namespace SlipeServer.Net.Managed;

/// <summary>
/// Wire reliability values — RakNet's <c>PacketReliability</c> as read by MTA:SA.
/// </summary>
/// <remarks>
/// The names are RakNet's, not Slipe's. <c>SlipeServer.Packets.Enums.PacketReliability</c> has the
/// same numeric values with two of the names shifted, so a direct cast is a no-op:
/// Slipe's <c>ReliableSequenced</c> (3) is RakNet's <c>RELIABLE_ORDERED</c>, and Slipe's
/// <c>ReliableSequencedCanDropPackets</c> (4) is RakNet's <c>RELIABLE_SEQUENCED</c>.
/// </remarks>
public enum WireReliability : byte
{
    Unreliable = 0,
    UnreliableSequenced = 1,
    Reliable = 2,
    ReliableOrdered = 3,
    ReliableSequenced = 4,
}

public static class WireReliabilityExtensions
{
    /// <summary>
    /// Only reliabilities 1, 3 and 4 carry the 5-bit ordering channel and the 32-bit ordering
    /// index; the other reliabilities omit both fields.
    /// </summary>
    public static bool CarriesOrdering(this WireReliability reliability) =>
        reliability is WireReliability.UnreliableSequenced
            or WireReliability.ReliableOrdered
            or WireReliability.ReliableSequenced;

    public static bool IsReliable(this WireReliability reliability) =>
        reliability is WireReliability.Reliable
            or WireReliability.ReliableOrdered
            or WireReliability.ReliableSequenced;
}

/// <summary>One RakNet packet queued for transmission.</summary>
public sealed class OutgoingPacket
{
    public required uint SequenceNumber { get; init; }

    public required WireReliability Reliability { get; init; }

    /// <summary>Ordering channel, 5 bits on the wire. Only meaningful when <see cref="Reliability"/> carries ordering.</summary>
    public byte OrderingChannel { get; init; }

    /// <summary>Ordering index, 32 bits on the wire. Only meaningful when <see cref="Reliability"/> carries ordering.</summary>
    public uint OrderingIndex { get; init; }

    /// <summary>Raw packet bytes: the leading wire id followed by the payload.</summary>
    public required byte[] Payload { get; init; }

    /// <summary>
    /// Split-packet id, a plain 16-bit field. Every part of one logical message shares it.
    /// Only written when <see cref="SplitPacketCount"/> is non-zero.
    /// </summary>
    public ushort SplitPacketId { get; init; }

    /// <summary>Zero-based index of this part among <see cref="SplitPacketCount"/>.</summary>
    public uint SplitPacketIndex { get; init; }

    /// <summary>Total number of parts, or zero when this record is not a split part.</summary>
    public uint SplitPacketCount { get; init; }

    /// <summary>True when the per-packet record carries the split header.</summary>
    public bool HasSplitData => SplitPacketCount != 0;

    /// <summary>Exact number of bits this packet will occupy inside a datagram.</summary>
    public int EncodedBitCount
    {
        get
        {
            int bits = 32 + 3;                                   // sequence number + reliability
            if (Reliability.CarriesOrdering())
            {
                bits += 5 + 32;                                  // ordering channel + ordering index
            }
            bits += 1;                                           // has-split flag
            if (HasSplitData)
            {
                bits += 16;                                      // split packet id, plain 16-bit
                bits += CompressedBitCount(SplitPacketIndex);    // compressed 32-bit
                bits += CompressedBitCount(SplitPacketCount);    // compressed 32-bit
            }
            bits += CompressedBitCount((ushort)(Payload.Length * 8));
            bits += 8 * Payload.Length;                          // payload, byte-aligned
            return bits;
        }
    }

    /// <summary>
    /// Size of every field in this record except the payload and the compressed <c>bitCount</c>,
    /// with each variable-width field counted at its longest encoding. Used to derive the split
    /// chunk size, so it must never under-estimate.
    /// </summary>
    internal static int FixedOverheadBits(WireReliability reliability, bool split)
    {
        int bits = 32 + 3;                                       // sequence number + reliability
        if (reliability.CarriesOrdering())
        {
            bits += 5 + 32;                                      // ordering channel + ordering index
        }
        bits += 1;                                               // has-split flag
        if (split)
        {
            bits += 16;                                          // split packet id
            bits += CompressedBitCount(uint.MaxValue);            // split packet index
            bits += CompressedBitCount(uint.MaxValue);            // split packet count
        }
        return bits;
    }

    /// <summary>
    /// Bits consumed by a <c>WriteCompressed</c> of a 16-bit value: one flag bit, then either the
    /// raw low 16 bits (high byte non-zero) or the 8-bit / 4-bit terminal form.
    /// </summary>
    internal static int CompressedBitCount(ushort value)
    {
        var b = new[] { (byte)value, (byte)(value >> 8) };
        int bits = 0;
        for (int i = 1; i > 0; i--)
        {
            bits++;
            if (b[i] != 0)
            {
                return bits + i * 8 + 8;
            }
        }
        return bits + ((b[0] & 0xF0) == 0 ? 1 + 4 : 1 + 8);
    }

    /// <summary>
    /// The same ladder for a 32-bit value. The split index and count are compressed 32-bit fields,
    /// so they cost up to 33 bits, not 17.
    /// </summary>
    internal static int CompressedBitCount(uint value)
    {
        var b = new[] { (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) };
        int bits = 0;
        for (int i = 3; i > 0; i--)
        {
            bits++;
            if (b[i] != 0)
            {
                return bits + i * 8 + 8;
            }
        }
        return bits + ((b[0] & 0xF0) == 0 ? 1 + 4 : 1 + 8);
    }
}

/// <summary>
/// Builds an outbound RakNet datagram: the header bit ladder followed by one or more per-packet records.
/// </summary>
public static class RakNetDatagramWriter
{
    /// <summary>
    /// The receive side rejects any payload whose byte count reaches 1492 (0x5D4), the size of its
    /// receive buffer. This value stays comfortably below that so a datagram fits a typical MTU.
    /// </summary>
    public const int MaxDatagramBytes = 1400;

    /// <summary>
    /// The receive side refuses a split header whose part count exceeds <c>0x4000</c> and then
    /// disconnects, so a logical packet can never be spread over more than this many datagrams.
    /// </summary>
    public const int MaxSplitParts = 0x4000;

    /// <summary>
    /// The largest payload one part can carry in a datagram of at most
    /// <see cref="MaxDatagramBytes"/> bytes, for the given reliability. Ordering reliabilities
    /// pay for the channel and ordering index; every part pays for the split header.
    /// </summary>
    /// <remarks>
    /// The compressed <c>bitCount</c> width depends on the chunk size, so the answer is a
    /// fixpoint: start from the budget minus the fixed overhead, then shrink until the encoded
    /// bit count also fits. The wire fixes no outbound MTU, so <see cref="MaxDatagramBytes"/> is
    /// this port's own choice, not a claim about the native implementation; it stays under the hard
    /// 1491-byte per-part ceiling the receive side enforces.
    /// </remarks>
    public static int MaxPayloadBytes(WireReliability reliability)
    {
        const int datagramHeaderBits = 1 + 64 + 1; // ack flag, send time, second-field flag
        int budgetBits = MaxDatagramBytes * 8 - datagramHeaderBits - OutgoingPacket.FixedOverheadBits(reliability, split: true);
        int bytes = Math.Min(budgetBits >> 3, 1491);
        while (bytes > 0 && bytes * 8 + OutgoingPacket.CompressedBitCount((ushort)(bytes * 8)) > budgetBits)
        {
            bytes--;
        }
        return Math.Max(bytes, 0);
    }

    /// <summary>
    /// Slices <paramref name="payload"/> into the fewest parts that each fit a datagram, tagging
    /// every part with the same split id and count. A payload that already fits comes
    /// back as one packet with the has-split flag clear, so callers never need to branch.
    /// </summary>
    /// <remarks>
    /// Every part takes its own sequence number; the ordering index is shared by the whole
    /// logical packet so the receive side reassembles and delivers it as one unit.
    /// </remarks>
    public static OutgoingPacket[] Split(
        byte[] payload,
        WireReliability reliability,
        ushort splitPacketId,
        uint orderingIndex,
        uint firstSequenceNumber,
        out uint nextSequenceNumber)
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (payload.Length == 0)
        {
            throw new ArgumentException("A RakNet packet payload cannot be empty.", nameof(payload));
        }

        int chunkBytes = MaxPayloadBytes(reliability);
        if (chunkBytes <= 0)
        {
            throw new ArgumentException($"A {reliability} packet leaves no room for a payload.", nameof(reliability));
        }

        int partCount = (payload.Length + chunkBytes - 1) / chunkBytes;
        if (partCount > MaxSplitParts)
        {
            throw new ArgumentException(
                $"Payload of {payload.Length} bytes needs {partCount} parts, over the {MaxSplitParts}-part ceiling.",
                nameof(payload));
        }

        if (partCount == 1)
        {
            nextSequenceNumber = firstSequenceNumber + 1;
            return
            [
                new OutgoingPacket
                {
                    SequenceNumber = firstSequenceNumber,
                    Reliability = reliability,
                    OrderingIndex = orderingIndex,
                    Payload = payload,
                },
            ];
        }

        var parts = new OutgoingPacket[partCount];
        uint sequence = firstSequenceNumber;
        for (int i = 0; i < partCount; i++)
        {
            int offset = i * chunkBytes;
            int length = Math.Min(chunkBytes, payload.Length - offset);
            var chunk = new byte[length];
            Array.Copy(payload, offset, chunk, 0, length);

            parts[i] = new OutgoingPacket
            {
                SequenceNumber = sequence++,
                Reliability = reliability,
                OrderingIndex = orderingIndex,
                Payload = chunk,
                SplitPacketId = splitPacketId,
                SplitPacketIndex = (uint)i,
                SplitPacketCount = (uint)partCount,
            };
        }

        nextSequenceNumber = sequence;
        return parts;
    }

    public static byte[] Build(
        ulong remoteSendTimeMs,
        IReadOnlyList<AckRange> acks,
        IReadOnlyList<OutgoingPacket> packets)
    {
        var bs = new BitStream(512 + packets.Count * 32);

        bool hasAcks = acks.Count > 0;
        bs.WriteBit(hasAcks);
        if (hasAcks)
        {
            bs.Write(remoteSendTimeMs);
            WriteAckBlock(bs, acks);
        }

        bs.WriteBit(false); // no second optional 64-bit field

        foreach (var packet in packets)
        {
            WritePacket(bs, packet);
        }

        return bs.ToArray();
    }

    /// <summary>
    /// Ack block: a compressed 16-bit range count, then per entry ONE flag bit — <c>1</c> = a single
    /// sequence number, <c>0</c> = a range (start and end). Stock RakNet uses two flag bits here;
    /// MTA's does not.
    /// </summary>
    private static void WriteAckBlock(BitStream bs, IReadOnlyList<AckRange> ranges)
    {
        bs.WriteCompressed((ushort)ranges.Count);
        foreach (var range in ranges)
        {
            if (range.Start == range.End)
            {
                bs.WriteBit(true);
                bs.Write(range.Start);
            }
            else
            {
                bs.WriteBit(false);
                bs.Write(range.Start);
                bs.Write(range.End);
            }
        }
    }

    /// <summary>The per-packet record: sequence number, reliability, optional ordering, split flag/fields, compressed bit count, aligned payload.</summary>
    private static void WritePacket(BitStream bs, OutgoingPacket packet)
    {
        int bitCount = packet.Payload.Length * 8;
        if (bitCount == 0)
        {
            throw new ArgumentException("A RakNet packet payload cannot be empty.", nameof(packet));
        }
        if ((bitCount + 7) >> 3 >= 0x5D4)
        {
            throw new ArgumentException($"Payload of {packet.Payload.Length} bytes exceeds the 1491-byte limit.", nameof(packet));
        }

        bs.Write(packet.SequenceNumber);

        // WriteBits shifts a partial chunk itself (b << (8 - chunk)), so pass the raw value.
        bs.WriteBits(new[] { (byte)((int)packet.Reliability & 0x07) }, 3, true);

        if (packet.Reliability.CarriesOrdering())
        {
            bs.WriteBits(new[] { (byte)(packet.OrderingChannel & 0x1F) }, 5, true);
            bs.Write(packet.OrderingIndex);
        }

        if (packet.HasSplitData)
        {
            bs.WriteBit(true);
            bs.Write(packet.SplitPacketId);                       // plain 16-bit, not compressed
            bs.WriteCompressed(packet.SplitPacketIndex);          // compressed 32-bit
            bs.WriteCompressed(packet.SplitPacketCount);          // compressed 32-bit
        }
        else
        {
            bs.WriteBit(false);                                   // no split
        }
        bs.WriteCompressed((ushort)bitCount);     // compressed 16-bit, not a plain write
        bs.WriteAlignedBytes(packet.Payload, packet.Payload.Length);
    }
}
