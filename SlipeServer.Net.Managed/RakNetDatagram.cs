namespace SlipeServer.Net.Managed;

/// <summary>One packet lifted out of a RakNet datagram.</summary>
public sealed class RakNetPacketRecord
{
    /// <summary>32-bit sequence number, plain.</summary>
    public uint SequenceNumber { get; init; }

    /// <summary>3-bit reliability, plain. Only 1, 3 and 4 carry ordering fields.</summary>
    public byte Reliability { get; init; }

    public byte OrderingChannel { get; init; }

    public uint OrderingIndex { get; init; }

    public ushort SplitPacketId { get; init; }

    public uint SplitPacketIndex { get; init; }

    public uint SplitPacketCount { get; init; }

    /// <summary>Payload length in <em>bits</em> (a compressed 16-bit field).</summary>
    public int BitCount { get; init; }

    /// <summary>
    /// The payload bytes, byte-aligned on the wire. Length is <c>(BitCount + 7) &gt;&gt; 3</c>.
    /// </summary>
    public byte[] Payload { get; init; } = Array.Empty<byte>();

    public bool HasSplitData => SplitPacketCount != 0;
}

/// <summary>An inclusive range of acknowledged sequence numbers.</summary>
public readonly record struct AckRange(uint Start, uint End);

/// <summary>
/// A parsed RakNet datagram: the header fields, the optional ack block and the per-packet records.
/// </summary>
public sealed class RakNetDatagram
{
    public bool HasAcks { get; private init; }

    /// <summary>The remote send time in <em>milliseconds</em>; present only when acks are.</summary>
    public ulong RemoteSendTimeMs { get; private init; }

    public IReadOnlyList<AckRange> Acks { get; private init; } = Array.Empty<AckRange>();

    public bool HasSecondField { get; private init; }

    /// <summary>An extra optional 64-bit header field added by MTA's RakNet fork.</summary>
    public ulong SecondField { get; private init; }

    public IReadOnlyList<RakNetPacketRecord> Packets { get; private init; } = Array.Empty<RakNetPacketRecord>();

    /// <summary>Parses a datagram; fails if it is under 3 bytes or any field is malformed.</summary>
    public static bool TryParse(byte[] datagram, int byteLength, out RakNetDatagram parsed, out string error)
    {
        parsed = new RakNetDatagram();
        error = string.Empty;

        if (byteLength < 3)
        {
            error = $"ERR length:{byteLength} buffer:short";
            return false;
        }

        var bs = new BitStream(datagram, 0, byteLength);

        bool hasAcks = bs.ReadBit();
        ulong remoteSendTimeMs = 0;
        var acks = new List<AckRange>();
        if (hasAcks)
        {
            if (!bs.Read(out remoteSendTimeMs))
            {
                error = "ERR remoteSendTime";
                return false;
            }
            if (!TryReadAckBlock(bs, acks))
            {
                error = "ERR incomingAcks.Deserialize";
                return false;
            }
        }

        bool hasSecond = bs.ReadBit();
        ulong second = 0;
        if (hasSecond && !bs.Read(out second))
        {
            error = "ERR secondField";
            return false;
        }

        var packets = new List<RakNetPacketRecord>();
        while (bs.GetNumberOfUnreadBits() >= 0x20)
        {
            if (!TryReadPacket(bs, out var record, out error))
            {
                return false;
            }
            packets.Add(record);
        }

        parsed = new RakNetDatagram
        {
            HasAcks = hasAcks,
            RemoteSendTimeMs = remoteSendTimeMs,
            Acks = acks,
            HasSecondField = hasSecond,
            SecondField = second,
            Packets = packets,
        };
        return true;
    }

    /// <summary>Ack block: <c>[16-bit rangeCount][per range: 1 bit single | 32-bit seq [| 32-bit seqEnd]]</c>.</summary>
    private static bool TryReadAckBlock(BitStream bs, List<AckRange> into)
    {
        if (!bs.ReadCompressed(out ushort rangeCount))
        {
            return false;
        }
        for (int i = 0; i < rangeCount; i++)
        {
            // One flag bit: 1 = single sequence number, 0 = a range. (Stock RakNet uses two bits.)
            bool isSingle = bs.ReadBit();
            if (!bs.Read(out uint start))
            {
                return false;
            }
            uint end = start;
            if (!isSingle)
            {
                if (!bs.Read(out end))
                {
                    return false;
                }
                if (end < start)
                {
                    return false;
                }
            }
            into.Add(new AckRange(start, end));
        }
        return true;
    }

    /// <summary>One packet record: sequence, reliability, optional ordering and split headers, payload.</summary>
    private static bool TryReadPacket(BitStream bs, out RakNetPacketRecord record, out string error)
    {
        error = string.Empty;
        record = null!;

        if (bs.GetNumberOfUnreadBits() < 0x20)
        {
            error = "ERR shortPacketHeader";
            return false;
        }

        if (!bs.Read(out uint sequenceNumber))
        {
            error = "ERR sequenceNumber";
            return false;
        }

        var reliabilityBits = new byte[1];
        if (!bs.ReadBits(reliabilityBits, 3, true))
        {
            error = "ERR reliability";
            return false;
        }
        byte reliability = reliabilityBits[0];

        byte orderingChannel = 0;
        uint orderingIndex = 0;
        if (reliability is 1 or 3 or 4)
        {
            var channelBits = new byte[1];
            if (!bs.ReadBits(channelBits, 5, true) || !bs.Read(out orderingIndex))
            {
                error = "ERR orderingFields";
                return false;
            }
            orderingChannel = channelBits[0];
        }

        ushort splitPacketId = 0;
        uint splitPacketIndex = 0;
        uint splitPacketCount = 0;
        if (bs.ReadBit())
        {
            if (!bs.Read(out splitPacketId) ||
                !bs.ReadCompressed(out splitPacketIndex) ||
                !bs.ReadCompressed(out splitPacketCount))
            {
                error = "ERR splitHeader";
                return false;
            }
            if (splitPacketCount == 0 || splitPacketCount <= splitPacketIndex || splitPacketCount > 0x4000)
            {
                error = $"ERR split index:{splitPacketIndex} count:{splitPacketCount}";
                return false;
            }
        }

        if (!bs.ReadCompressed(out ushort bitCount) || bitCount == 0)
        {
            error = "ERR bitCount";
            return false;
        }
        int byteCount = (bitCount + 7) >> 3;
        if (byteCount >= 0x5D4)
        {
            error = $"ERR payloadTooLarge bytes:{byteCount}";
            return false;
        }

        // Check the declared length against the bytes that remain before sizing a buffer, so a tiny
        // datagram claiming a large payload cannot force a large allocation.
        if (bs.GetNumberOfAlignedBytesRemaining() < byteCount)
        {
            error = $"ERR payloadShort bytes:{byteCount} unread:{bs.GetNumberOfAlignedBytesRemaining()}";
            return false;
        }

        var payload = new byte[byteCount];
        if (!bs.ReadAlignedBytes(payload, byteCount))
        {
            error = "ERR payloadRead";
            return false;
        }

        record = new RakNetPacketRecord
        {
            SequenceNumber = sequenceNumber,
            Reliability = reliability,
            OrderingChannel = orderingChannel,
            OrderingIndex = orderingIndex,
            SplitPacketId = splitPacketId,
            SplitPacketIndex = splitPacketIndex,
            SplitPacketCount = splitPacketCount,
            BitCount = bitCount,
            Payload = payload,
        };
        return true;
    }
}
