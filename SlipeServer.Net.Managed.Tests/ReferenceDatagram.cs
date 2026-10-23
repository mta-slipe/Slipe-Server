using System;
using System.Collections.Generic;
using System.IO;

using SlipeServer.Net.Managed;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// One packet as recovered by <see cref="ReferenceDatagram"/>.
/// </summary>
internal sealed record ReferencePacket(
    uint Sequence,
    int Reliability,
    int Channel,
    uint OrderingIndex,
    bool HasSplit,
    ushort SplitId,
    uint SplitIndex,
    uint SplitCount,
    int BitCount,
    byte[] Payload);

/// <summary>
/// One datagram as recovered by <see cref="ReferenceDatagram"/>.
/// </summary>
internal sealed record ReferenceParsedDatagram(
    IReadOnlyList<(uint Start, uint End)> Acks,
    bool HasSecondField,
    IReadOnlyList<ReferencePacket> Packets);

/// <summary>
/// A second, independent implementation of the datagram framing ladder, written from the documented
/// framing and the stock-<c>BitStream</c> primitives rather than from the port's own types.
///
/// <para>
/// This exists so the replay test can state what <c>capture.log</c> <em>should</em> decode to without
/// asking the port. It shares no code with <c>BitStream</c>, <c>RakNetDatagram</c> or
/// <c>RakNetDatagramWriter</c>: agreement between this and the port is therefore evidence that both
/// read the documented layout the same way, which is what a conformance oracle needs. Disagreement
/// is a finding either way.
/// </para>
///
/// <para>
/// It is a reader only, deliberately. Nothing here can produce wire bytes, so it can never be used to
/// generate an input for the port — the failure mode the suite exists to prevent.
/// </para>
/// </summary>
internal static class ReferenceDatagram
{
    /// <summary>The MTA message layer's own floor, distinct from the reliability layer's 3 bytes.</summary>
    internal const int MinimumDatagramBytes = 8;

    /// <summary>Stock <c>MAXIMUM_MTU_SIZE</c> payload ceiling, <c>(bitCount + 7) >> 3 &gt;= 0x5D4</c> fails.</summary>
    internal const int MaximumPayloadBytes = 0x5D4 - 1;

    /// <summary>RakNet's <c>RID_USER_PACKET_ENUM</c>; ids below it are system messages.</summary>
    internal const byte RakNetUserPacketEnum = 99;

    internal static bool TryParse(byte[] datagram, out ReferenceParsedDatagram? parsed, out string error)
    {
        parsed = null;

        if (datagram.Length < 3)
        {
            error = $"length:{datagram.Length} buffer:short";
            return false;
        }

        try
        {
            return TryParseCore(datagram, out parsed, out error);
        }
        catch (InvalidDataException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static bool TryParseCore(byte[] datagram, out ReferenceParsedDatagram? parsed, out string error)
    {
        parsed = null;
        error = string.Empty;

        var reader = new ReferenceBitReader(datagram);

        var acks = new List<(uint Start, uint End)>();
        bool hasAcks = reader.ReadBit();
        if (hasAcks)
        {
            reader.ReadRun(64); // the sender's timestamp in milliseconds

            int rangeCount = (int)ToValue(reader.ReadCompressed(16));
            for (int i = 0; i < rangeCount; i++)
            {
                bool single = reader.ReadBit();
                uint start = (uint)reader.ReadValue(32);
                uint end = single ? start : (uint)reader.ReadValue(32);
                if (end < start)
                {
                    error = $"ack range {start}..{end}";
                    return false;
                }

                acks.Add((start, end));
            }
        }

        bool hasSecondField = reader.ReadBit();
        if (hasSecondField)
            reader.ReadRun(64);

        var packets = new List<ReferencePacket>();
        while (reader.UnreadBits >= 0x20)
        {
            uint sequence = (uint)reader.ReadValue(32);
            int reliability = (int)reader.ReadValue(3);

            int channel = 0;
            uint orderingIndex = 0;
            if (reliability is 1 or 3 or 4)
            {
                channel = (int)reader.ReadValue(5);
                orderingIndex = (uint)reader.ReadValue(32);
            }

            bool hasSplit = reader.ReadBit();
            ushort splitId = 0;
            uint splitIndex = 0;
            uint splitCount = 0;
            if (hasSplit)
            {
                splitId = (ushort)reader.ReadValue(16);
                splitIndex = ToValue(reader.ReadCompressed(32));
                splitCount = ToValue(reader.ReadCompressed(32));
                if (splitCount == 0 || splitCount <= splitIndex || splitCount > 0x4000)
                {
                    error = $"split index:{splitIndex} count:{splitCount}";
                    return false;
                }
            }

            int bitCount = (int)ToValue(reader.ReadCompressed(16));
            if (bitCount == 0)
            {
                error = "bit count 0";
                return false;
            }

            int byteCount = (bitCount + 7) >> 3;
            if (byteCount > MaximumPayloadBytes)
            {
                error = $"payload {byteCount} B";
                return false;
            }

            packets.Add(new ReferencePacket(
                sequence,
                reliability,
                channel,
                orderingIndex,
                hasSplit,
                splitId,
                splitIndex,
                splitCount,
                bitCount,
                reader.ReadAlignedBytes(byteCount)));
        }

        error = string.Empty;
        parsed = new ReferenceParsedDatagram(acks, hasSecondField, packets);
        return true;
    }

    /// <summary>
    /// <c>mtaId = ResolveInboundId(wireId - 99)</c>: the <c>+99</c> offset, then the build-dependent
    /// <c>4 ⇄ 0x13</c> step. Returns null for ids the MTA layer never sees: everything below
    /// <c>RID_USER_PACKET_ENUM</c>, and the two reserved MTA ids.
    /// </summary>
    internal static byte? MtaIdOf(byte wireId, MtaIdDialect dialect = MtaIdDialect.Release)
    {
        if (wireId < RakNetUserPacketEnum)
            return null;

        byte mtaId = ResolveInboundId((byte)(wireId - RakNetUserPacketEnum), dialect);
        return mtaId is 0 or 0xFF ? null : mtaId;
    }

    /// <summary>
    /// The inbound id step, written out independently of <c>MtaProtocol.ResolveInboundId</c> so that a
    /// change to one cannot silently validate itself through the other.
    /// </summary>
    internal static byte ResolveInboundId(byte resolved, MtaIdDialect dialect)
    {
        if (dialect == MtaIdDialect.Debug)
            return resolved;

        // Release swaps, Both collapses 0x13 onto 4, and Auto's pre-latch behaviour is Both.
        if (dialect == MtaIdDialect.Release)
            return SwapIds(resolved);

        return resolved == 0x13 ? (byte)4 : resolved;
    }

    /// <summary>
    /// The release build's <c>4 ⇄ 0x13</c> remap, an involution. The debug build has no such step, and
    /// egress applies no remap in either build.
    /// </summary>
    internal static byte SwapIds(byte id) => id switch
    {
        4 => 0x13,
        0x13 => 4,
        _ => id,
    };

    /// <summary>
    /// The three-id whitelist, evaluated on the stripped payload.
    /// </summary>
    internal static bool IsWhitelisted(byte mtaId, byte[] strippedPayload)
    {
        if (mtaId is 0x22 or 0x6C)
            return true;

        return mtaId == 0x18 && strippedPayload.Length >= 2 && strippedPayload[1] is 0 or 1;
    }

    private static uint ToValue(byte[] littleEndian)
    {
        uint value = 0;
        for (int i = littleEndian.Length - 1; i >= 0; i--)
            value = (value << 8) | littleEndian[i];

        return value;
    }
}

/// <summary>
/// MSB-first bit reader over a raw datagram. Written from the documented primitive rather than
/// reused from the port; see <see cref="ReferenceDatagram"/>.
/// </summary>
/// <remarks>
/// Two conventions of this build are load-bearing and are the reason this type exists separately:
/// a bit run of <c>n</c> bits is read most-significant-chunk first, with the <em>first</em> 8-bit chunk
/// landing in element 0, so multi-byte values come out little-endian; and the compressed-int
/// ladder terminates on the flag bit itself, so a value below <c>0x10</c> costs 6 bits, below
/// <c>0x100</c> 10 bits, and anything larger 17 (for a 16-bit width).
/// </remarks>
internal sealed class ReferenceBitReader
{
    private readonly byte[] buffer;
    private readonly int bitLength;
    private int offset;

    internal ReferenceBitReader(byte[] buffer)
    {
        this.buffer = buffer;
        this.bitLength = buffer.Length * 8;
    }

    internal int UnreadBits => this.bitLength - this.offset;

    internal bool ReadBit()
    {
        // Mirrors BitStream.ReadBit: past the end is a silent false, not a failure.
        if (this.offset >= this.bitLength)
            return false;

        bool value = (this.buffer[this.offset >> 3] & (0x80 >> (this.offset & 7))) != 0;
        this.offset++;
        return value;
    }

    /// <summary>
    /// Reads <paramref name="numBits"/> bits into <c>(numBits + 7) &gt;&gt; 3</c> bytes, first chunk
    /// first, with a short final chunk right-aligned in its byte.
    /// </summary>
    internal byte[] ReadRun(int numBits)
    {
        // Mirrors BitStream.ReadBits: a multi-bit read that would cross the end fails outright.
        if (this.offset + numBits > this.bitLength)
            throw new InvalidDataException($"bit read past the end ({numBits} at {this.offset}/{this.bitLength})");

        var result = new byte[(numBits + 7) >> 3];

        int remaining = numBits;
        int index = 0;
        while (remaining > 0)
        {
            int chunk = Math.Min(8, remaining);
            int value = 0;
            for (int i = 0; i < chunk; i++)
                value = (value << 1) | (this.ReadBit() ? 1 : 0);

            result[index] = chunk < 8 ? (byte)value : (byte)(value << (8 - chunk));
            remaining -= chunk;
            index++;
        }

        return result;
    }

    /// <summary>A typed field: <see cref="ReadRun"/> reinterpreted little-endian.</summary>
    internal ulong ReadValue(int numBits) => ToValue(this.ReadRun(numBits));

    /// <summary>
    /// This build's <c>ReadCompressed</c> ladder. One flag bit per high byte: set means "this byte is
    /// the fill byte and the value is narrower", clear means "the remaining bytes follow now". The
    /// terminal flag is not a byte flag — set reads a bare nibble, clear reads a full byte.
    /// </summary>
    internal byte[] ReadCompressed(int numBits)
    {
        int byteCount = numBits >> 3;
        var result = new byte[byteCount];

        for (int i = byteCount - 1; i > 0; i--)
        {
            if (!this.ReadBit())
            {
                this.ReadRun(i * 8 + 8).CopyTo(result, 0);
                return result;
            }

            result[i] = 0x00;
        }

        bool narrow = this.ReadBit();
        result[0] = this.ReadRun(narrow ? 4 : 8)[0];
        return result;
    }

    internal byte[] ReadAlignedBytes(int count)
    {
        this.offset = (this.offset + 7) & ~7;

        var result = new byte[count];
        for (int i = 0; i < count; i++)
            result[i] = (byte)this.ReadValue(8);

        return result;
    }

    private static uint ToValue(byte[] littleEndian)
    {
        uint value = 0;
        for (int i = littleEndian.Length - 1; i >= 0; i--)
            value = (value << 8) | littleEndian[i];

        return value;
    }
}
