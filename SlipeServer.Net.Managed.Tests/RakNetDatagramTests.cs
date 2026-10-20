using SlipeServer.Net.Managed;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Round-trips our own datagram writer through our own parser, and pins the documented
/// byte-level shapes from findings/reliability-datagram-framing.md §13.1–§13.3.
/// </summary>
public class RakNetDatagramTests
{
    private static OutgoingPacket Packet(uint seq, WireReliability rel, byte channel, uint ordering, params byte[] payload) =>
        new()
        {
            SequenceNumber = seq,
            Reliability = rel,
            OrderingChannel = channel,
            OrderingIndex = ordering,
            Payload = payload,
        };

    [Fact]
    public void SingleReliableOrderedPacket_RoundTrips()
    {
        var payload = new byte[] { 0xCA }.Concat(System.Text.Encoding.ASCII.GetBytes("MtaNetPoc!")).ToArray();
        var datagram = RakNetDatagramWriter.Build(
            remoteSendTimeMs: 0x0123456789ABCDEFul,
            acks: new[] { new AckRange(0, 0) },
            packets: new[] { Packet(1, WireReliability.ReliableOrdered, 0, 1, payload) });

        Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out var parsed, out var error), error);

        Assert.True(parsed.HasAcks);
        Assert.Equal(0x0123456789ABCDEFul, parsed.RemoteSendTimeMs);
        Assert.Single(parsed.Acks);
        Assert.Equal(new AckRange(0, 0), parsed.Acks[0]);
        Assert.False(parsed.HasSecondField);

        Assert.Single(parsed.Packets);
        var p = parsed.Packets[0];
        Assert.Equal(1u, p.SequenceNumber);
        Assert.Equal((byte)WireReliability.ReliableOrdered, p.Reliability);
        Assert.Equal(0, p.OrderingChannel);
        Assert.Equal(1u, p.OrderingIndex);
        Assert.Equal(payload.Length * 8, p.BitCount);
        Assert.Equal(payload, p.Payload);
        Assert.False(p.HasSplitData);
    }

    [Theory]
    [InlineData(WireReliability.Unreliable, false)]
    [InlineData(WireReliability.UnreliableSequenced, true)]
    [InlineData(WireReliability.Reliable, false)]
    [InlineData(WireReliability.ReliableOrdered, true)]
    [InlineData(WireReliability.ReliableSequenced, true)]
    public void EveryReliability_RoundTrips(WireReliability rel, bool carriesOrdering)
    {
        Assert.Equal(carriesOrdering, rel.CarriesOrdering());

        var datagram = RakNetDatagramWriter.Build(
            1000,
            new[] { new AckRange(7, 7) },
            new[] { Packet(42, rel, carriesOrdering ? (byte)3 : (byte)0, 99, 0xDE, 0xAD) });

        Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out var parsed, out var error), error);
        var p = Assert.Single(parsed.Packets);
        Assert.Equal((byte)rel, p.Reliability);
        Assert.Equal(42u, p.SequenceNumber);
        if (carriesOrdering)
        {
            Assert.Equal(3, p.OrderingChannel);
            Assert.Equal(99u, p.OrderingIndex);
        }
        else
        {
            Assert.Equal(0u, p.OrderingIndex);
        }
        Assert.Equal(new byte[] { 0xDE, 0xAD }, p.Payload);
    }

    [Fact]
    public void MultiRangeAckBlock_RoundTrips()
    {
        var acks = new[] { new AckRange(1, 1), new AckRange(4, 9), new AckRange(20, 20) };
        var datagram = RakNetDatagramWriter.Build(
            77,
            acks,
            new[] { Packet(0, WireReliability.Reliable, 0, 0, 0x11) });

        Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out var parsed, out var error), error);
        Assert.Equal(acks, parsed.Acks.ToArray());
    }

    [Fact]
    public void AckBlockUsesOneFlagBitPerEntry_SingleIsTrue()
    {
        // One range {0,0}: WriteCompressed(1) = 6 bits, then a single flag bit = 1, then 32 bits.
        // Header total: 1 (ack flag) + 64 (send time) + 6 + 1 + 32 = 104 bits before the
        // second optional-field flag.
        var bs = new BitStream();
        bs.WriteCompressed((ushort)1);
        bs.WriteBit(true);
        bs.Write(0u);
        Assert.Equal(6 + 1 + 32, bs.NumberOfBitsUsed);

        var writer = RakNetDatagramWriter.Build(0, new[] { new AckRange(0, 0) }, Array.Empty<OutgoingPacket>());
        // 1 + 64 + 39 + 1 (second flag) = 105 bits -> 14 bytes.
        Assert.Equal(14, writer.Length);
    }

    [Fact]
    public void NoAcks_LeavesAckFlagClear()
    {
        var datagram = RakNetDatagramWriter.Build(
            0,
            Array.Empty<AckRange>(),
            new[] { Packet(1, WireReliability.ReliableOrdered, 0, 1, 0xAA) });

        // The ack flag is the first bit, so a datagram without acks must have a clear top bit.
        Assert.Equal(0, datagram[0] & 0x80);

        Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out var parsed, out var error), error);
        Assert.False(parsed.HasAcks);
        Assert.Empty(parsed.Acks);
        Assert.Single(parsed.Packets);
    }

    [Fact]
    public void MultiplePackets_RoundTrip()
    {
        var datagram = RakNetDatagramWriter.Build(
            5,
            new[] { new AckRange(2, 2) },
            new[]
            {
                Packet(10, WireReliability.ReliableOrdered, 0, 10, 0x01, 0x02),
                Packet(11, WireReliability.Unreliable, 0, 0, 0x03),
                Packet(12, WireReliability.ReliableSequenced, 2, 12, 0x04, 0x05, 0x06),
            });

        Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out var parsed, out var error), error);
        Assert.Equal(3, parsed.Packets.Count);
        Assert.Equal(10u, parsed.Packets[0].SequenceNumber);
        Assert.Equal(11u, parsed.Packets[1].SequenceNumber);
        Assert.Equal(12u, parsed.Packets[2].SequenceNumber);
        Assert.Equal(2, parsed.Packets[2].OrderingChannel);
        Assert.Equal(new byte[] { 0x04, 0x05, 0x06 }, parsed.Packets[2].Payload);
    }

    [Fact]
    public void ShortDatagram_IsRejected()
    {
        Assert.False(RakNetDatagram.TryParse(new byte[] { 0x00, 0x01 }, 2, out _, out _));
    }

    [Fact]
    public void TruncatedPayload_IsRejected()
    {
        var datagram = RakNetDatagramWriter.Build(
            0,
            new[] { new AckRange(0, 0) },
            new[] { Packet(1, WireReliability.ReliableOrdered, 0, 1, 0xAA, 0xBB, 0xCC) });

        // Chop the last two bytes off: the declared bit count can no longer be satisfied.
        Assert.False(RakNetDatagram.TryParse(datagram, datagram.Length - 2, out _, out _));
    }

    [Fact]
    public void Build_StillRefusesASingleOversizePacket()
    {
        // Production never reaches this: SendWirePayload runs payloads through Split first. The
        // guard stays as a last-ditch assertion on the per-part limit from §13.3.
        var tooBig = new byte[0x5D4];
        Assert.Throws<ArgumentException>(() => RakNetDatagramWriter.Build(
            0,
            Array.Empty<AckRange>(),
            new[] { Packet(1, WireReliability.Reliable, 0, 0, tooBig) }));
    }

    [Fact]
    public void Split_PayloadThatFits_ReturnsOneUnsplittablePart()
    {
        byte[] payload = Payload(RakNetDatagramWriter.MaxPayloadBytes(WireReliability.ReliableOrdered));

        var parts = RakNetDatagramWriter.Split(
            payload,
            WireReliability.ReliableOrdered,
            splitPacketId: 7,
            orderingIndex: 42,
            firstSequenceNumber: 100,
            out uint nextSequenceNumber);

        var part = Assert.Single(parts);
        Assert.False(part.HasSplitData);
        Assert.Equal(100u, part.SequenceNumber);
        Assert.Equal(101u, nextSequenceNumber);
        Assert.Equal(42u, part.OrderingIndex);
        Assert.Same(payload, part.Payload);
    }

    [Fact]
    public void Split_OversizePayload_ProducesPartsThatEachRoundTrip()
    {
        int chunkBytes = RakNetDatagramWriter.MaxPayloadBytes(WireReliability.ReliableOrdered);
        byte[] payload = Payload(chunkBytes * 2 + 17);
        const ushort splitId = 0x1234;

        var parts = RakNetDatagramWriter.Split(
            payload,
            WireReliability.ReliableOrdered,
            splitId,
            orderingIndex: 9,
            firstSequenceNumber: 500,
            out uint nextSequenceNumber);

        Assert.Equal(3, parts.Length);
        Assert.Equal(503u, nextSequenceNumber);

        for (int i = 0; i < parts.Length; i++)
        {
            byte[] datagram = RakNetDatagramWriter.Build(
                0,
                Array.Empty<AckRange>(),
                new[] { parts[i] });

            Assert.True(datagram.Length <= RakNetDatagramWriter.MaxDatagramBytes, $"part {i} is {datagram.Length} bytes");
            Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out var parsed, out var error), error);

            var record = Assert.Single(parsed.Packets);
            Assert.True(record.HasSplitData);
            Assert.Equal(splitId, record.SplitPacketId);
            Assert.Equal((uint)i, record.SplitPacketIndex);
            Assert.Equal((uint)parts.Length, record.SplitPacketCount);
            Assert.Equal((uint)(500 + i), record.SequenceNumber);
            Assert.Equal(9u, record.OrderingIndex);
            Assert.Equal(parts[i].Payload, record.Payload);
        }
    }

    [Fact]
    public void Split_PartsReassembleToTheOriginalPayload()
    {
        int chunkBytes = RakNetDatagramWriter.MaxPayloadBytes(WireReliability.Reliable);
        byte[] payload = Payload(chunkBytes * 3 - 1);

        var parts = RakNetDatagramWriter.Split(
            payload,
            WireReliability.Reliable,
            splitPacketId: 1,
            orderingIndex: 0,
            firstSequenceNumber: 0,
            out _);

        Assert.Equal(3, parts.Length);

        var reassembled = new List<byte>();
        foreach (var part in parts)
        {
            reassembled.AddRange(part.Payload);
        }

        Assert.Equal(payload, reassembled.ToArray());
    }

    [Fact]
    public void Split_SequenceNumbersAreContiguousAndOrderingIndexIsShared()
    {
        int chunkBytes = RakNetDatagramWriter.MaxPayloadBytes(WireReliability.ReliableSequenced);
        var parts = RakNetDatagramWriter.Split(
            Payload(chunkBytes * 4),
            WireReliability.ReliableSequenced,
            splitPacketId: 3,
            orderingIndex: 77,
            firstSequenceNumber: 1000,
            out uint nextSequenceNumber);

        Assert.Equal(4, parts.Length);
        Assert.Equal(1004u, nextSequenceNumber);
        for (int i = 0; i < parts.Length; i++)
        {
            Assert.Equal((uint)(1000 + i), parts[i].SequenceNumber);
            Assert.Equal(77u, parts[i].OrderingIndex);
            Assert.Equal((ushort)3, parts[i].SplitPacketId);
            Assert.Equal((uint)i, parts[i].SplitPacketIndex);
            Assert.Equal(4u, parts[i].SplitPacketCount);
        }
    }

    [Fact]
    public void Split_EmptyPayload_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => RakNetDatagramWriter.Split(
            Array.Empty<byte>(),
            WireReliability.Reliable,
            splitPacketId: 0,
            orderingIndex: 0,
            firstSequenceNumber: 0,
            out _));
    }

    [Fact]
    public void Split_UnreliablePayload_AlsoFitsADatagram()
    {
        // Unreliable carries no ordering fields, so its chunk is slightly larger than the
        // ordering reliabilities'. The budget must still hold for every part.
        int chunkBytes = RakNetDatagramWriter.MaxPayloadBytes(WireReliability.Unreliable);
        Assert.True(chunkBytes >= RakNetDatagramWriter.MaxPayloadBytes(WireReliability.Reliable));

        var parts = RakNetDatagramWriter.Split(
            Payload(chunkBytes + 1),
            WireReliability.Unreliable,
            splitPacketId: 2,
            orderingIndex: 0,
            firstSequenceNumber: 0,
            out _);

        Assert.Equal(2, parts.Length);
        foreach (var part in parts)
        {
            byte[] datagram = RakNetDatagramWriter.Build(0, Array.Empty<AckRange>(), new[] { part });
            Assert.True(datagram.Length <= RakNetDatagramWriter.MaxDatagramBytes);
            Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out var parsed, out var error), error);
            Assert.Single(parsed.Packets);
        }
    }

    private static byte[] Payload(int length)
    {
        var payload = new byte[length];
        for (int i = 0; i < length; i++)
        {
            payload[i] = (byte)((i * 31) + 7);
        }

        return payload;
    }

    [Fact]
    public void EmptyPayload_IsRefusedByTheWriter()
    {
        Assert.Throws<ArgumentException>(() => RakNetDatagramWriter.Build(
            0,
            Array.Empty<AckRange>(),
            new[] { Packet(1, WireReliability.Reliable, 0, 0, Array.Empty<byte>()) }));
    }
}
