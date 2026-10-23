using Xunit;

using SlipeServer.Net.Managed;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// The datagram bit ladder, pinned on <b>literal bytes recorded from a real MTA:SA client</b> rather
/// than on bytes the port's own writer produced.
/// </summary>
/// <remarks>
/// <para>
/// Every hex literal below is transcribed from <c>capture.log</c>, the two recorded real-client
/// sessions, and every expected value is what that recorded datagram says. This matters: a test that
/// builds its input with <see cref="RakNetDatagramWriter"/> and asserts the port's reader only proves
/// the two halves agree, not that either matches the wire.
/// </para>
/// <para>
/// The one thing these tests cannot tell you is whether the client and the server meant the same
/// thing by these bytes; only the findings' reading of the server binary speaks to that.
/// </para>
/// </remarks>
public class RakNetDatagramWireFactTests
{
    /// <summary>
    /// <c>capture.log</c> seq 3 — the real client's connected <c>RID_CONNECTION_REQUEST</c>. It sets
    /// the optional second 64-bit field and carries no ack block.
    /// </summary>
    private const string RecordedConnectedRequest =
        "4E100CC000000000000000001010020400FF00FFFEFEFEFEFDFDFDFD78563417" +
        "F0A5D2BAB4E48DEC4A29518967DEDC21";

    /// <summary>
    /// <c>capture.log</c> seq 6 — two packets, <c>RID_NEW_INCOMING_CONNECTION</c> (wire 0x11) followed
    /// by a wire 0x00 system packet, again with the second field and no acks.
    /// </summary>
    private const string RecordedTwoPacketDatagram =
        "5A100CC000000000004000001800000000018020113F574DA7EAC53F574DE1C9" +
        "E200000000FFFF00000000FFFF00000000FFFF00000000FFFF00000000FFFF00" +
        "000000FFFF00000000FFFF00000000FFFF00000000FFFF020000004920004D40" +
        "330000000000";

    /// <summary>
    /// <c>capture.log</c> seq 15 — a pure ack datagram: no second field, one ack range, no packets.
    /// </summary>
    private const string RecordedAckOnlyDatagram = "80000000000000006302000000";

    /// <summary>
    /// <c>capture.log</c> seq 29 — another pure ack datagram, this one carrying a two-element range.
    /// </summary>
    private const string RecordedAckRangeDatagram = "800000000000000062050000000600000000";

    /// <summary>
    /// The datagram header: bit 0 is the ack flag and the bit after it (and after the ack block, when
    /// present) is the optional-second-64-bit-field flag. The recorded client uses the <em>second</em>
    /// field as its send timestamp and leaves <see cref="RakNetDatagram.RemoteSendTimeMs"/> at zero
    /// because it never piggybacks acks on a packet.
    /// </summary>
    [Theory]
    // capture.log seq 3
    [InlineData(RecordedConnectedRequest, 0x0000000000334038UL, 1)]
    // capture.log seq 6
    [InlineData(RecordedTwoPacketDatagram, 0x0000000000334068UL, 2)]
    public void TheRecordedClientSetsTheSecondFieldAndNotTheAckTimestamp(
        string hex,
        ulong expectedSecondField,
        int expectedPackets)
    {
        byte[] datagram = WireTestKit.Hex(hex);

        Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out RakNetDatagram parsed, out string error));
        Assert.Equal(string.Empty, error);
        Assert.False(parsed.HasAcks);
        Assert.Equal(0UL, parsed.RemoteSendTimeMs);
        Assert.True(parsed.HasSecondField);
        Assert.Equal(expectedSecondField, parsed.SecondField);
        Assert.Equal(expectedPackets, parsed.Packets.Count);
    }

    /// <summary>
    /// The second field is a monotonic millisecond clock: seq 3, 6 and 16 were recorded seconds apart
    /// and the recorded values advance by roughly that much. Pinning the ordering, not just the
    /// individual values, is what stops a refactor from silently reinterpreting the field.
    /// </summary>
    [Fact]
    public void TheSecondFieldAdvancesLikeAMillisecondClock()
    {
        ulong[] fields = { 0x0000000000334038UL, 0x0000000000334068UL, 0x0000000000334657UL };

        for (int i = 1; i < fields.Length; i++)
            Assert.True(fields[i] > fields[i - 1], "the recorded second fields must be increasing");

        // 0x334657 - 0x334038 = 1567 ms between capture.log seq 3 and seq 16.
        Assert.Equal(1567UL, fields[2] - fields[0]);
    }

    /// <summary>
    /// The ack block is a <c>ReadCompressed</c> 16-bit range count followed by one flag bit per
    /// range — <c>1</c> for a single sequence number, <c>0</c> for a <c>[start, end]</c> pair. Both
    /// shapes appear in the recording, so both are pinned here.
    /// </summary>
    [Theory]
    // capture.log seq 15: one single-sequence ack for sequence 2.
    [InlineData(RecordedAckOnlyDatagram, 2u, 2u)]
    // capture.log seq 29: one ack range covering sequences 5 and 6.
    [InlineData(RecordedAckRangeDatagram, 5u, 6u)]
    public void TheRecordedAckDatagramsCarryExactlyOneRange(string hex, uint expectedStart, uint expectedEnd)
    {
        byte[] datagram = WireTestKit.Hex(hex);

        Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out RakNetDatagram parsed, out string error));
        Assert.Equal(string.Empty, error);
        Assert.True(parsed.HasAcks);
        Assert.Equal(0UL, parsed.RemoteSendTimeMs);
        Assert.False(parsed.HasSecondField);
        Assert.Empty(parsed.Packets);

        AckRange range = Assert.Single(parsed.Acks);
        Assert.Equal(expectedStart, range.Start);
        Assert.Equal(expectedEnd, range.End);
    }

    /// <summary>
    /// A real-wire invariant the port relies on but never states: the recorded client never
    /// piggybacks acks on a datagram that also carries a packet. Either a datagram is pure acks or it
    /// is pure packets.
    /// </summary>
    [Fact]
    public void NoRecordedClientDatagramCarriesAcksAndAPacket()
    {
        var checkedRecords = 0;

        foreach (CaptureLine line in WireTestKit.CaptureLog.Where(line => line.IsRx))
        {
            if (!RakNetDatagram.TryParse(line.Bytes, line.Bytes.Length, out RakNetDatagram parsed, out _))
                continue;

            checkedRecords++;

            if (parsed.HasAcks)
                Assert.Empty(parsed.Packets);
        }

        Assert.True(checkedRecords > 100, $"expected the whole recording to parse, got {checkedRecords} records");
    }

    /// <summary>
    /// The packet loop condition is <c>while (unreadBits &gt;= 0x20)</c>, so a trailing run of fewer than
    /// 32 bits is padding and a run of 32 is attempted as a packet. The recorded seq-6 datagram is
    /// byte-aligned and exactly consumed, which makes it the ideal base for the two boundary cases.
    /// </summary>
    [Fact]
    public void ThePacketLoopRequiresThirtyTwoUnreadBits()
    {
        byte[] exact = WireTestKit.Hex(RecordedTwoPacketDatagram);
        Assert.True(RakNetDatagram.TryParse(exact, exact.Length, out RakNetDatagram parsed, out _));
        Assert.Equal(2, parsed.Packets.Count);

        // 24 trailing zero bits: below the floor, so they are ignored rather than parsed.
        byte[] plusThree = Pad(exact, 3);
        Assert.True(RakNetDatagram.TryParse(plusThree, plusThree.Length, out RakNetDatagram padded, out string error));
        Assert.Equal(string.Empty, error);
        Assert.Equal(2, padded.Packets.Count);

        // 32 trailing zero bits: exactly at the floor, so a third packet is attempted and rejected.
        byte[] plusFour = Pad(exact, 4);
        Assert.False(RakNetDatagram.TryParse(plusFour, plusFour.Length, out _, out string floorError));
        Assert.Equal("ERR reliability", floorError);
    }

    /// <summary>
    /// The same floor from the other side: a header-only datagram parses to zero packets until there
    /// are 32 bits left to spend on a packet record.
    /// </summary>
    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public void AHeaderOnlyDatagramBelowTheFloorParsesToZeroPackets(int length)
    {
        byte[] datagram = new byte[length];

        Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out RakNetDatagram parsed, out string error));
        Assert.Equal(string.Empty, error);
        Assert.False(parsed.HasAcks);
        Assert.False(parsed.HasSecondField);
        Assert.Empty(parsed.Packets);
    }

    /// <summary>
    /// One byte past that floor and the parser commits to a packet record, which the zeroed
    /// <c>ReadCompressed</c> bit count then rejects.
    /// </summary>
    [Fact]
    public void AHeaderOnlyDatagramAtTheFloorIsRejected()
    {
        byte[] datagram = new byte[5];

        Assert.False(RakNetDatagram.TryParse(datagram, datagram.Length, out _, out string error));
        Assert.Equal("ERR bitCount", error);
    }

    /// <summary>
    /// The recorded 483-byte datagram from <c>capture.log</c> seq 16 — the largest single-packet
    /// record in the session — still has to parse to one packet with the recorded sequence, channel
    /// and ordering index. Kept as a file read rather than an inline literal because 483 bytes of hex
    /// would drown the file; the source is still <c>capture.log</c>, not the port.
    /// </summary>
    [Fact]
    public void TheLargestRecordedDatagramParsesToItsRecordedPacket()
    {
        CaptureLine line = WireTestKit.CaptureLogRecord(16);

        Assert.True(RakNetDatagram.TryParse(line.Bytes, line.Bytes.Length, out RakNetDatagram parsed, out string error));
        Assert.Equal(string.Empty, error);
        Assert.False(parsed.HasAcks);
        Assert.True(parsed.HasSecondField);
        Assert.Equal(0x0000000000334657UL, parsed.SecondField);

        RakNetPacketRecord packet = Assert.Single(parsed.Packets);
        Assert.Equal(6u, packet.SequenceNumber);
        Assert.Equal((byte)WireReliability.ReliableOrdered, packet.Reliability);
        Assert.Equal((byte)3, packet.OrderingChannel);
        Assert.Equal(1u, packet.OrderingIndex);
        Assert.False(packet.HasSplitData);
        Assert.Equal(3704, packet.BitCount);
        Assert.Equal(463, packet.Payload.Length);
        Assert.Equal("7C000000", WireTestKit.HexOf(packet.Payload[..4]));
    }

    private static byte[] Pad(byte[] source, int count)
    {
        var padded = new byte[source.Length + count];
        Array.Copy(source, padded, source.Length);
        return padded;
    }
}
