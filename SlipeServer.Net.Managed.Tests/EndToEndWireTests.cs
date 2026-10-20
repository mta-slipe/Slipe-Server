using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// End-to-end tests that feed <em>literal</em> wire bytes in at the transport boundary
/// (<see cref="MtaNetWrapper.InjectDatagram"/>) and assert on the observable
/// <see cref="INetWrapper"/> outcome: the <c>PacketReceived</c> five-tuple and the outbound bytes
/// recorded by <c>Capture</c>.
///
/// <para>
/// <b>What makes these evidence, and what does not.</b> Every input array here was transcribed by
/// hand from <c>capture.log</c> (datagrams a real MTA:SA client and the stock server actually sent)
/// or from a byte layout worked out by hand. None was produced by <c>BitStream</c>,
/// <c>RakNetDatagramWriter</c>, <c>MtaGamePackets</c> or any other part of the port. That matters
/// because a test that feeds the port's own writer into the port's own reader shares every
/// misreading between the two and cancels out — it certifies that the port is identical to itself,
/// not that it is wire compatible.
/// </para>
///
/// <para>
/// So: the tests in this file are evidence about the <b>inbound parse path</b> and about the
/// <b>shape of the bytes the port emits</b>. They are <em>not</em> proof that a stock client agrees —
/// only a real client run can show that. The companion file
/// <c>LoopbackConsistencyTests.cs</c> covers the synthetic-client case and is explicitly
/// internal-consistency only; do not confuse the two, and never describe a green run of either as a
/// compatibility result.
/// </para>
///
/// <para>
/// Golden vectors live in <see cref="CaptureVectors"/> with a per-vector citation;
/// <see cref="CaptureVectorProvenanceTests"/> re-derives each one from its cited source so a
/// transcription slip cannot survive the suite.
/// </para>
/// </summary>
public class EndToEndWireTests
{
    // ================================================================ stage 1: datagram framing
    // A literal datagram from the recording must yield the framing the capture layer recorded for
    // it. The expected values below are the capture.log classification tokens, not a parse of the
    // port's own output.

    /// <summary>
    /// capture.log seq 4: <c>datagram acks=1 pkts=1 second=False [p0 wire=0x0E seq=0 rel=2 ch=0
    /// oi=0 bits=552 bytes=69]</c>. Pins the ladder entry for a datagram that carries an ack
    /// block and no second optional field.
    /// </summary>
    [Fact]
    public void RecordedReplyWithAnAckBlockFramesAsOneReliablePacket()
    {
        byte[] datagramBytes = CaptureVectors.ConnectedReplyStockBytes;

        Assert.True(
            RakNetDatagram.TryParse(datagramBytes, datagramBytes.Length, out RakNetDatagram datagram, out string error),
            error);

        Assert.True(datagram.HasAcks);
        Assert.False(datagram.HasSecondField);

        AckRange ack = Assert.Single(datagram.Acks);
        Assert.Equal(0u, ack.Start);
        Assert.Equal(0u, ack.End);

        RakNetPacketRecord packet = Assert.Single(datagram.Packets);
        Assert.Equal(0u, packet.SequenceNumber);
        Assert.Equal(2, packet.Reliability);
        Assert.Equal(0, packet.OrderingChannel);
        Assert.Equal(0u, packet.OrderingIndex);
        Assert.False(packet.HasSplitData);
        Assert.Equal(552, packet.BitCount);
        Assert.Equal(69, packet.Payload.Length);
        Assert.Equal(CaptureVectors.ConnectedReplyBodyRecorded, WireTestKit.HexOf(packet.Payload));
    }

    /// <summary>
    /// capture.log seq 16: <c>datagram acks=0 pkts=1 second=True [p0 wire=0x7C seq=6 rel=3 ch=3
    /// oi=1 bits=3704 bytes=463]</c>. The only recorded datagram with the second optional 64-bit
    /// field set, so it is the only literal that exercises that ladder rung.
    /// </summary>
    [Fact]
    public void RecordedDatagramWithTheSecondOptionalFieldFramesAsOneOrderedPacket()
    {
        byte[] datagramBytes = WireTestKit.CaptureLogRecord(16).Bytes;

        Assert.True(
            RakNetDatagram.TryParse(datagramBytes, datagramBytes.Length, out RakNetDatagram datagram, out string error),
            error);

        Assert.False(datagram.HasAcks);
        Assert.Empty(datagram.Acks);
        Assert.True(datagram.HasSecondField);

        RakNetPacketRecord packet = Assert.Single(datagram.Packets);
        Assert.Equal(6u, packet.SequenceNumber);
        Assert.Equal(3, packet.Reliability);
        Assert.Equal(3, packet.OrderingChannel);
        Assert.Equal(1u, packet.OrderingIndex);
        Assert.Equal(3704, packet.BitCount);
        Assert.Equal(463, packet.Payload.Length);
    }

    /// <summary>
    /// capture.log seq 18: two packets, <c>seq=7 bits=16 bytes=2</c> and <c>seq=8 bits=24 bytes=3</c>,
    /// neither with an ack block nor a second field. Pins the <c>&gt;= 32 unread bits</c> loop
    /// boundary: the second packet's 32-bit header plus 24 payload bits is exactly the minimum that
    /// keeps the loop going.
    /// </summary>
    [Fact]
    public void RecordedDatagramCarriesTwoPacketsInOrder()
    {
        byte[] datagramBytes = WireTestKit.CaptureLogRecord(18).Bytes;

        Assert.True(
            RakNetDatagram.TryParse(datagramBytes, datagramBytes.Length, out RakNetDatagram datagram, out string error),
            error);

        Assert.Equal(2, datagram.Packets.Count);

        Assert.Equal(7u, datagram.Packets[0].SequenceNumber);
        Assert.Equal(16, datagram.Packets[0].BitCount);
        Assert.Equal(2, datagram.Packets[0].Payload.Length);
        Assert.Equal("7B00", WireTestKit.HexOf(datagram.Packets[0].Payload));

        Assert.Equal(8u, datagram.Packets[1].SequenceNumber);
        Assert.Equal(24, datagram.Packets[1].BitCount);
        Assert.Equal(3, datagram.Packets[1].Payload.Length);
        Assert.Equal("7B0300", WireTestKit.HexOf(datagram.Packets[1].Payload));
    }

    /// <summary>
    /// capture.log seq 15: <c>datagram acks=1 pkts=0 second=False</c> — a datagram that carries only
    /// an ack block and no packets at all. A parser that assumes at least one packet either throws
    /// or invents one here.
    /// </summary>
    [Fact]
    public void RecordedAckOnlyDatagramParsesToNoPackets()
    {
        byte[] datagramBytes = WireTestKit.CaptureLogRecord(15).Bytes;

        Assert.True(
            RakNetDatagram.TryParse(datagramBytes, datagramBytes.Length, out RakNetDatagram datagram, out string error),
            error);

        Assert.True(datagram.HasAcks);
        Assert.NotEmpty(datagram.Acks);
        Assert.Empty(datagram.Packets);
    }

    /// <summary>
    /// The entry guard: fewer than 3 bytes cannot even hold the flags byte, the "has acks" bit
    /// and a 32-bit packet header, so the parse must fail rather than read past the buffer.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("00")]
    [InlineData("0000")]
    public void DatagramsBelowTheThreeByteFloorAreRejected(string hex)
    {
        byte[] datagramBytes = WireTestKit.Hex(hex);

        Assert.False(
            RakNetDatagram.TryParse(datagramBytes, datagramBytes.Length, out RakNetDatagram datagram, out string error));
        Assert.Equal($"ERR length:{datagramBytes.Length} buffer:short", error);
        Assert.Empty(datagram.Packets);
    }

    // ================================================================ stage 2: wire id -> MTA id

    /// <summary>
    /// The wire-id to MTA-id mapping plus <c>MtaProtocol.SwapIds</c>: the first payload byte is the wire id, the MTA
    /// id is <c>wire - 99</c> with the <c>4 ⇄ 0x13</c> pair swapped back. The wire ids are the ones
    /// the recording actually contains, so a refactor that reorders the swap or changes the offset
    /// fails here.
    /// </summary>
    [Theory]
    [InlineData(0x63, 0)]     // SERVER_JOIN — capture.log never carries it, but the offset is pinned
    [InlineData(0x65, 2)]     // SERVER_JOIN_COMPLETE — capture.log seq 14
    [InlineData(0x66, 3)]     // PLAYER_JOIN — capture.log seq 6 delivers this via RakNet 0x11
    [InlineData(0x76, 4)]     // PLAYER_JOINDATA — capture.log seq 12 (0x76 -> swap -> 0x13 -> +99)
    [InlineData(0x67, 19)]    // RakNet DISCONNECTION_NOTIFICATION (0x13) after the swap
    [InlineData(0x7C, 25)]    // PLAYER_LIST — capture.log seq 12 and 16
    [InlineData(0x7B, 24)]    // RPC — capture.log seq 18 and 23
    [InlineData(0x83, 32)]    // PLAYER_PURESYNC — capture.log seq 20
    [InlineData(0xB8, 85)]    // PACKET_ID_CAMERA_SYNC — capture.log seq 87
    public void WireIdMapsToTheZeroBasedMtaId(byte wireId, byte expectedMtaId)
    {
        // The forward direction is what the port's own encoder must produce...
        Assert.Equal(wireId, MtaProtocol.ToWireId(expectedMtaId));

        // ...and the reverse direction is what an inbound literal must decode to. Deriving it here
        // from the two documented steps rather than calling a port helper keeps this independent.
        byte swapped = MtaProtocol.SwapIds((byte)(wireId - MtaProtocol.MtaIdToWireId));
        Assert.Equal(expectedMtaId, swapped);
    }

    /// <summary>
    /// The swap is an involution on the two ids it touches and the identity everywhere else, which is
    /// what makes <c>wire - 99</c> reversible. Pinned because a refactor that replaces the swap with
    /// a table can easily lose the identity arm.
    /// </summary>
    [Fact]
    public void TheIdSwapTouchesOnlyFourAndNineteen()
    {
        Assert.Equal(4, MtaProtocol.SwapIds(0x13));
        Assert.Equal(0x13, MtaProtocol.SwapIds(4));

        for (int id = 0; id < 256; id++)
        {
            if (id is 4 or 0x13)
                continue;

            Assert.Equal((byte)id, MtaProtocol.SwapIds((byte)id));
        }
    }

    // ================================================================ offline replies from literals

    /// <summary>
    /// capture.log seq 1 (the real client's <c>RID_OPEN_CONNECTION_REQUEST</c>) must produce the
    /// 33-byte <c>[0x0A][magic : 16][server GUID : 16]</c> reply. The GUID is the wrapper's own, so the reply shape is asserted field by field rather
    /// than compared to the recorded reply.
    /// </summary>
    [Fact]
    public void RecordedOpenConnectionRequestProducesTheExactThirtyThreeByteReply()
    {
        using var harness = new WrapperHarness();
        int txBefore = harness.Tx.Count;

        harness.Inject(CaptureVectors.OpenRequestBytes);

        byte[] reply = NewestRawTx(harness, txBefore);
        Assert.Equal(33, reply.Length);
        Assert.Equal(0x0A, reply[0]);
        Assert.Equal("00FF00FFFEFEFEFEFDFDFDFD78563417", WireTestKit.HexOf(reply[1..17]));
        Assert.Equal(WireTestKit.HexOf(harness.Wrapper.ServerGuid), WireTestKit.HexOf(reply[17..33]));
        Assert.NotEqual(WireTestKit.HexOf(CaptureVectors.StockServerGuid), WireTestKit.HexOf(reply[17..33]));
    }

    /// <summary>
    /// An offline <c>RID_PING</c> built from the documented layout must produce
    /// the 41-byte pong: <c>[0x1A][client time : u64 LE][server GUID : 16][magic : 16]</c>. Note the
    /// reply id is <c>0x1A</c> (<c>RID_PONG</c>), not the MTA-specific <c>0x26</c> message — a
    /// refactor that "tidies" those two together breaks every client.
    /// </summary>
    [Fact]
    public void PingProducesTheExactFortyOneBytePongWithTheClientTimeEchoed()
    {
        using var harness = new WrapperHarness();
        int txBefore = harness.Tx.Count;

        harness.Inject(CaptureVectors.PingRequestBytes);

        byte[] pong = NewestRawTx(harness, txBefore);
        Assert.Equal(41, pong.Length);
        Assert.Equal(0x1A, pong[0]);

        // 0x1122334455667788 little-endian.
        Assert.Equal("8877665544332211", WireTestKit.HexOf(pong[1..9]));
        Assert.Equal(CaptureVectors.PingClientTime, BitConverter.ToUInt64(pong, 1));

        Assert.Equal(WireTestKit.HexOf(harness.Wrapper.ServerGuid), WireTestKit.HexOf(pong[9..25]));
        Assert.Equal("00FF00FFFEFEFEFEFDFDFDFD78563417", WireTestKit.HexOf(pong[25..41]));
    }

    /// <summary>
    /// The config-failed arm: an <c>RID_OPEN_CONNECTION_REQUEST</c> whose version byte is <c>0x03</c>
    /// instead of <c>0x04</c> gets the 34-byte <c>[0x5F][0x04][magic][GUID]</c> refusal.
    /// The extra <c>0x04</c> is what distinguishes it from
    /// the two 33-byte refusals below.
    /// </summary>
    [Fact]
    public void WrongVersionOpenConnectionRequestProducesTheExactThirtyFourByteRefusal()
    {
        using var harness = new WrapperHarness();
        int txBefore = harness.Tx.Count;

        harness.InjectHex(CaptureVectors.OpenConnectionRequestBadVersion);

        byte[] refusal = NewestRawTx(harness, txBefore);
        Assert.Equal(34, refusal.Length);
        Assert.Equal(0x5F, refusal[0]);
        Assert.Equal(0x04, refusal[1]);
        Assert.Equal("00FF00FFFEFEFEFEFDFDFDFD78563417", WireTestKit.HexOf(refusal[2..18]));
        Assert.Equal(WireTestKit.HexOf(harness.Wrapper.ServerGuid), WireTestKit.HexOf(refusal[18..34]));
    }

    /// <summary>
    /// The no-slots arm: with <c>playerCount = 0</c> the same recorded request gets the 33-byte
    /// <c>[0x12][magic][GUID]</c> refusal. Driven by the same
    /// literal as the accepted case, which is what makes it a controlled comparison.
    /// </summary>
    [Fact]
    public void OpenConnectionRequestWithNoFreeSlotsProducesTheExactThirtyThreeByteRefusal()
    {
        using var harness = new WrapperHarness(playerCount: 0);
        int txBefore = harness.Tx.Count;

        harness.Inject(CaptureVectors.OpenRequestBytes);

        byte[] refusal = NewestRawTx(harness, txBefore);
        Assert.Equal(33, refusal.Length);
        Assert.Equal(0x12, refusal[0]);
        Assert.Equal("00FF00FFFEFEFEFEFDFDFDFD78563417", WireTestKit.HexOf(refusal[1..17]));
        Assert.Equal(WireTestKit.HexOf(harness.Wrapper.ServerGuid), WireTestKit.HexOf(refusal[17..33]));

        Assert.Equal(0, harness.Wrapper.SessionCount);
    }

    /// <summary>
    /// A peer that already holds a session record gets no second reply — the native arm returns
    /// silently once a session record exists for the peer. Replaying the same
    /// recorded request twice must therefore produce exactly one datagram, not two.
    /// </summary>
    [Fact]
    public void ARepeatedOpenConnectionRequestFromTheSamePeerIsSilent()
    {
        using var harness = new WrapperHarness();

        harness.Inject(CaptureVectors.OpenRequestBytes);
        int afterFirst = harness.Tx.Count;
        Assert.Equal(1, afterFirst);

        harness.Inject(CaptureVectors.OpenRequestBytes);

        Assert.Equal(afterFirst, harness.Tx.Count);
        Assert.Contains(harness.Trace, line => line.Contains("already-connected", StringComparison.Ordinal));
    }

    // ================================================================ connected handshake + delivery

    /// <summary>
    /// capture.log seq 3 (the real client's connected <c>RID_CONNECTION_REQUEST</c>, wire id
    /// <c>0x04</c>) must produce the 69-byte <c>RID_CONNECTION_REQUEST_ACCEPTED</c> body the stock
    /// server sent in seq 4. With a wildcard host the port advertises no server address, so the ten
    /// 6-byte slots serialize as <c>FFFFFFFF0000</c> and the body is byte-identical to the recording.
    /// </summary>
    [Fact]
    public void RecordedConnectedConnectionRequestProducesTheExactSixtyNineByteReply()
    {
        using var harness = new WrapperHarness();
        harness.OpenSession();
        int txBefore = harness.Tx.Count;

        harness.Inject(CaptureVectors.ConnectedRequestBytes);

        byte[] reply = PayloadOfTheNewestTx(harness, txBefore, expectedWireId: 0x0E);
        Assert.Equal(69, reply.Length);
        Assert.Equal(CaptureVectors.ConnectedReplyBodyStock, WireTestKit.HexOf(reply));

        // Field by field, so a refactor cannot satisfy the equality by echoing the recording:
        // the client address dword is complemented and the port is the recording's. The dword is
        // little-endian on the wire, so the byte run "3FFFFDF5" is the value 0xF5FDFF3F.
        Assert.Equal(0x0E, reply[0]);
        Assert.Equal("3FFFFDF5", WireTestKit.HexOf(reply[1..5]));
        Assert.Equal(0xF5FD_FF3Fu, BitConverter.ToUInt32(reply, 1));
        Assert.Equal(0x0A02_00C0u, ~BitConverter.ToUInt32(reply, 1));
        Assert.Equal(58057, BitConverter.ToUInt16(reply, 5));
        Assert.Equal(0, BitConverter.ToUInt16(reply, 7));
        Assert.Equal("FFFFFFFF0000", WireTestKit.HexOf(reply[9..15]));
    }

    /// <summary>
    /// capture.log seq 6: the client's <c>RID_NEW_INCOMING_CONNECTION</c> (wire <c>0x11</c>) is
    /// synthesised into MTA <c>PACKET_ID_PLAYER_JOIN</c> (mtaId 3) with the RakNet id byte stripped,
    /// and the trailing RakNet system packet in the same datagram produces nothing at all.
    /// </summary>
    [Fact]
    public void RecordedIncomingConnectionDeliversPlayerJoinAndIgnoresTheSystemPacket()
    {
        using var harness = new WrapperHarness();
        harness.OpenSession();

        harness.InjectHex(CaptureVectors.IncomingConnectionAndSystemPacket);

        Received received = Assert.Single(harness.Received);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_JOIN, received.Id);
        Assert.Equal(66, received.Data.Length);

        // [client address complemented][10 slots], with the 0x11 id byte removed.
        Assert.Equal("3F574DA7EAC5", WireTestKit.HexOf(received.Data[..6]));
        Assert.Equal("3F574DE1C9E2", WireTestKit.HexOf(received.Data[6..12]));
    }

    /// <summary>
    /// capture.log seq 20: <c>wire=0x83 seq=9 rel=3 ch=0 oi=4 bits=224 bytes=28</c>. The datagram's
    /// first byte is <c>0x5F</c>, which matches an offline refusal's message id but not its 34-byte
    /// length, so the offline router hands it to the connected path rather than refusing it
    /// (<c>magic@2 MISMATCH len=47 expected=34</c>). Delivery must carry the 0-based mtaId 32 and the
    /// 27-byte stripped payload.
    /// </summary>
    [Fact]
    public void RecordedRpcDeliversTheZeroBasedIdAndTheStrippedPayload()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.InjectHex(CaptureVectors.RpcPacketWire83);

        Received received = Assert.Single(harness.Received);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_PURESYNC, received.Id);
        Assert.Equal(32, (int)received.Id);
        Assert.Equal(
            "0000000000180000000000000000000001006400008807D13B0000",
            WireTestKit.HexOf(received.Data));
        Assert.Equal(27, received.Data.Length);
    }

    /// <summary>
    /// capture.log seq 12: the real client's <c>PLAYER_JOINDATA</c> (wire <c>0x76</c>). This is the
    /// single best case for proving the strip, because the recording gives the pre-strip bytes: the
    /// packet payload is 871 bytes, the wire id takes one, and the mod-level join body only starts
    /// 775 bytes into the remaining 870, so <c>ExtractPlayerJoinDataBlock</c> has to drop that opaque
    /// net-layer prefix before the event fires. The serial
    /// lives in the prefix, so it has to be read <em>before</em> the strip.
    /// </summary>
    [Fact]
    public void RecordedPlayerJoinDataIsStrippedAndItsSerialAndVersionAreExtracted()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.Inject(WireTestKit.CaptureLogRecord(12).Bytes);

        Assert.Equal(2, harness.Received.Count);

        Received joinData = harness.Received[0];
        Assert.Equal(PacketId.PACKET_ID_PLAYER_JOINDATA, joinData.Id);

        // The 95-byte mod-level body: netVersion, an opaque u16, the bitstream version, the
        // length-prefixed version string, then the (removed) community block and the trailing dwords.
        Assert.Equal(
            "DA01600191000F00312E362E302D392E32343134392E3085A9A0A2A99F2730B737A630B83A37B8" +
            "000000000000000000000000000000000000000000000000000000000000000000000000000000" +
            "00000000078000006460A4151F2730B700",
            WireTestKit.HexOf(joinData.Data));
        Assert.Equal(95, joinData.Data.Length);

        // The body's own leading fields, read out of the literal above.
        Assert.Equal(0x01DA, BitConverter.ToUInt16(joinData.Data, 0));
        Assert.Equal(0x0160, BitConverter.ToUInt16(joinData.Data, 2));
        Assert.Equal(0x0091, BitConverter.ToUInt16(joinData.Data, 4));
        Assert.Equal("1.6.0-9.24149.0", System.Text.Encoding.ASCII.GetString(joinData.Data, 8, 15));

        Received playerList = harness.Received[1];
        Assert.Equal(PacketId.PACKET_ID_PLAYER_LIST, playerList.Id);

        Tuple<string, string, string> identity = harness.Wrapper.GetClientSerialExtraAndVersion(harness.Address);
        Assert.Equal("5DDE026C7AD482CF6D73B7B3B460FD5E", identity.Item1);
        Assert.Equal("1.6.0-9.24149.0", identity.Item3);
    }

    /// <summary>
    /// capture.log seq 303: <c>wire=0x13 seq=109 rel=3 ch=0 oi=97 bits=8 bytes=1</c>, the client's
    /// <c>RID_DISCONNECTION_NOTIFICATION</c>. The native adapter maps <c>0x13</c> to
    /// <c>PACKET_ID_PLAYER_QUIT</c> with the id byte stripped, leaving a zero-length payload because
    /// the system message carries no body.
    /// </summary>
    [Fact]
    public void RecordedDisconnectionNotificationDeliversPlayerQuitWithAnEmptyPayload()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.Inject(CaptureVectors.DisconnectionNotificationBytes);

        Received received = Assert.Single(harness.Received);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_QUIT, received.Id);
        Assert.Empty(received.Data);
    }

    /// <summary>
    /// capture.log seq 87: <c>[p0 wire=0x83 ...]</c> then <c>[p1 wire=0xB8 seq=... bits=32
    /// bytes=4]</c> — one datagram, two different MTA ids (32 then 85), so a refactor that caches the
    /// id per datagram instead of per packet fails.
    /// </summary>
    [Fact]
    public void RecordedDatagramWithTwoDifferentIdsDeliversBothInOrder()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.Inject(WireTestKit.CaptureLogRecord(87).Bytes);

        Assert.Equal(2, harness.Received.Count);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_PURESYNC, harness.Received[0].Id);
        Assert.Equal(PacketId.PACKET_ID_CAMERA_SYNC, harness.Received[1].Id);
        Assert.Equal(85, (int)harness.Received[1].Id);
        Assert.Equal("01010000", WireTestKit.HexOf(harness.Received[1].Data));
    }

    // ================================================================ negative cases

    /// <summary>
    /// A datagram below <see cref="MtaAdmission.MinimumDatagramBytes"/> never reaches the parser: no
    /// event, no reply, no exception.
    /// </summary>
    [Theory]
    [InlineData("00")]
    [InlineData("00000000000000")]      // 7 bytes: one below the floor
    [InlineData("0000000000000000")]    // 8 bytes: at the floor but an unparseable header
    public void ShortOrUnparseableConnectedDatagramsAreDroppedSilently(string hex)
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();
        int txBefore = harness.Tx.Count;

        harness.InjectHex(hex);

        Assert.Empty(harness.Received);

        // Only the ack flush, if any, may go out; nothing is answered and the session survives.
        Assert.True(harness.Tx.Count - txBefore <= 1);
        Assert.Equal(WrapperHarness.CaptureLogPeer.Address, harness.Wrapper.GetPlayerIp(harness.Address));
    }

    /// <summary>
    /// The reliability layer acks a retransmission but does not re-deliver it. The same recorded
    /// datagram injected twice must therefore raise exactly one event while still being acked twice —
    /// the sequence set is per session.
    /// </summary>
    [Fact]
    public void DuplicateSequenceNumbersAreReAckedButNotReDelivered()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.InjectHex(CaptureVectors.RpcPacketWire83);
        Assert.Single(harness.Received);

        harness.InjectHex(CaptureVectors.RpcPacketWire83);

        Assert.Single(harness.Received);
        Assert.Contains(harness.Trace, line => line.Contains("duplicate seq 9", StringComparison.Ordinal));

        // The ack for the retransmission still went out, so the peer stops retrying.
        Assert.NotEmpty(harness.Tx);
    }

    /// <summary>
    /// The mtaId-4 version gate: once <see cref="MtaNetWrapper.SetVersion"/> has recorded a negotiated
    /// bit-stream version, an inbound <c>PLAYER_JOINDATA</c> is refused. The drop is silent — no event, no reply, no disconnect — and the session survives.
    /// </summary>
    [Fact]
    public void JoinDataIsDroppedOnceAVersionHasBeenNegotiated()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();
        harness.Wrapper.SetVersion(harness.Address, 0x90);

        harness.Inject(WireTestKit.CaptureLogRecord(12).Bytes);

        // The second packet in the datagram (PLAYER_LIST) is unaffected; only the join data is refused.
        Assert.DoesNotContain(harness.Received, received => received.Id == PacketId.PACKET_ID_PLAYER_JOINDATA);
        Assert.Contains(harness.Received, received => received.Id == PacketId.PACKET_ID_PLAYER_LIST);
        Assert.Contains(harness.Trace, line => line.Contains("dropped wire id 0x76", StringComparison.Ordinal));
        Assert.Equal(WrapperHarness.CaptureLogPeer.Address, harness.Wrapper.GetPlayerIp(harness.Address));
    }

    /// <summary>
    /// Gate 2's usage budget latches a flood notice and then refuses everything for that session.
    /// This is the only reachable silent-drop path that does not depend on a negotiated version, so it
    /// is the one that pins "no event, no reply, no disconnect" for a post-handshake rejection.
    /// </summary>
    [Fact]
    public void GateTwoExhaustionLatchesAFloodNoticeAndThenDropsSilently()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        // mtaId 32 is PLAYER_PURESYNC; the packet budget is inclusive (<=), so 2 lets two through
        // and latches on the third. All three inputs are recorded datagrams, not built ones.
        harness.Wrapper.AdmissionOptions.Gate2PacketBudgets[32] = 2;

        harness.InjectHex(CaptureVectors.RpcPacketWire83);          // capture.log seq 20, count 1
        Assert.Single(harness.Received);

        harness.Inject(WireTestKit.CaptureLogRecord(87).Bytes);     // capture.log seq 87, count 2
        Assert.Equal(3, harness.Received.Count);

        int txBefore = harness.Tx.Count;
        harness.Inject(WireTestKit.CaptureLogRecord(89).Bytes);     // capture.log seq 89, count 3

        Assert.Equal(3, harness.Received.Count);
        Assert.Contains(harness.Trace, line => line.Contains("gate 2", StringComparison.Ordinal));
        Assert.True(harness.Tx.Count - txBefore <= 1);
        Assert.Equal(WrapperHarness.CaptureLogPeer.Address, harness.Wrapper.GetPlayerIp(harness.Address));
    }

    /// <summary>
    /// A connected-path datagram from a peer with no session record is discarded before parsing. This
    /// is the "unknown peer" arm of <c>HandleConnected</c> and it must not create a session, reply, or
    /// throw.
    /// </summary>
    [Fact]
    public void ConnectedDatagramsFromAnUnknownPeerAreDiscarded()
    {
        using var harness = new WrapperHarness();

        harness.InjectHex(CaptureVectors.RpcPacketWire83);

        Assert.Empty(harness.Received);
        Assert.Empty(harness.Tx);
        Assert.Equal(0, harness.Wrapper.SessionCount);
        Assert.Equal(IPAddress.None, harness.Wrapper.GetPlayerIp(harness.Address));
    }

    /// <summary>
    /// capture.log seq 15 is an ack-only datagram. Fed to a live session it must produce no event and
    /// must not disturb the sequence state, so the next real packet still delivers.
    /// </summary>
    [Fact]
    public void AnAckOnlyDatagramProducesNoEventAndDoesNotDisturbTheSession()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.Inject(WireTestKit.CaptureLogRecord(15).Bytes);
        Assert.Empty(harness.Received);

        harness.InjectHex(CaptureVectors.RpcPacketWire83);
        Assert.Single(harness.Received);
    }

    // ================================================================ the full replay

    /// <summary>
    /// The highest-value test in the suite: drive <b>every recorded Rx datagram</b> of both real
    /// MTA:SA client sessions, in order, through <see cref="MtaNetWrapper.InjectDatagram"/> and assert
    /// the resulting <c>PacketReceived</c> sequence and payloads against a hand-transcribed literal
    /// table.
    ///
    /// <para>
    /// Each row is <c>(capture.log record, 0-based mtaId, exact stripped payload hex)</c>. The table
    /// was produced by decoding <c>capture.log</c> by hand against the documented layouts, not by
    /// running the port, so a refactor that changes admission, stripping or id mapping fails here.
    /// </para>
    ///
    /// <para>
    /// The table deliberately does <b>not</b> assert the number or content of outbound datagrams. The
    /// stock server answers some packets this port does not (and vice versa), and ack-only datagrams
    /// interleave unpredictably, so a Tx assertion would encode the recording's timing rather than the
    /// contract. Outbound shape is pinned by the focused tests above instead.
    /// </para>
    /// </summary>
    [Fact]
    public void ReplayOfTheRecordedSessionsEmitsExactlyTheRecordedPackets()
    {
        using var harness = new WrapperHarness();

        IReadOnlyList<CaptureLine> rx = WireTestKit.CaptureLog.Where(line => line.IsRx).ToList();
        Assert.Equal(139, rx.Count);

        // Every cited record must really be a recorded Rx datagram, so the table cannot drift.
        var rxSequenceNumbers = rx.Select(line => line.Seq).ToHashSet();
        Assert.All(Replay, row => Assert.Contains(row.Record, rxSequenceNumbers));

        foreach (CaptureLine line in rx)
            harness.Inject(line.Bytes);

        Assert.Equal(Replay.Length, harness.Received.Count);

        for (int i = 0; i < Replay.Length; i++)
        {
            (int record, byte mtaId, string payload) = Replay[i];
            Received received = harness.Received[i];

            Assert.Same(harness.Wrapper, received.Sender);
            Assert.Equal(harness.Address, received.Address);
            Assert.Equal((PacketId)mtaId, received.Id);
            Assert.Null(received.Ping);

            string actualPayload = WireTestKit.HexOf(received.Data);
            Assert.True(
                payload == actualPayload,
                $"capture.log seq {record}, event {i}: expected mtaId {mtaId} payload {payload}, got {(int)received.Id} {actualPayload}");
        }
    }

    /// <summary>
    /// The same replay, checked against the second independent decoder. Where
    /// <see cref="ReplayOfTheRecordedSessionsEmitsExactlyTheRecordedPackets"/> pins <em>what</em> came
    /// out, this pins <em>why</em>: every delivered payload must be the body of some packet in the
    /// datagram with only its wire id removed — except <c>PLAYER_JOINDATA</c>, whose opaque net-layer
    /// prefix the strip drops. If the port and the table were both wrong in the same way, this still
    /// agrees, but it does catch a refactor that changes the strip width while keeping the table
    /// intact.
    /// </summary>
    [Fact]
    public void EveryReplayedPayloadIsTheDatagramPayloadMinusItsWireId()
    {
        using var harness = new WrapperHarness();

        var expected = new List<(int Record, string Payload)>();
        foreach (CaptureLine line in WireTestKit.CaptureLog.Where(l => l.IsRx))
        {
            harness.Inject(line.Bytes);

            if (!ReferenceDatagram.TryParse(line.Bytes, out ReferenceParsedDatagram? parsed, out _))
                continue;

            foreach (ReferencePacket packet in parsed!.Packets)
            {
                if (packet.Payload.Length == 0)
                    continue;

                // A RakNet system message only becomes an MTA packet for the adapter's five arms
                // (0x11, 0x13, 0x14, 0x21, 0x22); this capture only contains the first two.
                if (packet.Payload[0] < ReferenceDatagram.RakNetUserPacketEnum
                    && packet.Payload[0] is not (0x11 or 0x13 or 0x14))
                {
                    continue;
                }

                expected.Add((line.Seq, WireTestKit.HexOf(packet.Payload[1..])));
            }
        }

        var delivered = harness.Received
            .Select(received => WireTestKit.HexOf(received.Data))
            .ToList();

        Assert.Equal(Replay.Length, delivered.Count);

        for (int i = 0; i < delivered.Count; i++)
        {
            string actual = delivered[i];
            (int record, byte mtaId, string tablePayload) = Replay[i];

            Assert.True(tablePayload == actual, $"event {i}: table payload does not match the delivery");

            bool matches = expected.Any(candidate => candidate.Record == record && candidate.Payload == actual);

            if (!matches && mtaId == MtaGamePackets.PlayerJoinDataMtaId)
            {
                // The join body is a proper suffix of the net payload: the strip only ever drops a
                // prefix. The exact 95-byte tail is pinned literally by
                // RecordedPlayerJoinDataIsStrippedAndItsSerialAndVersionAreExtracted.
                matches = expected.Any(candidate =>
                    candidate.Record == record
                    && candidate.Payload.Length > actual.Length
                    && candidate.Payload.EndsWith(actual, StringComparison.Ordinal));
            }

            Assert.True(
                matches,
                $"event {i}: capture.log seq {record} mtaId {mtaId} delivered {actual.Length / 2}B, " +
                "which is not the body of any packet in that record");
        }
    }

    // ---------------------------------------------------------------- helpers

    /// <summary>
    /// Returns the newest Tx record verbatim, asserting that exactly one new datagram was written
    /// since <paramref name="txBefore"/>.
    /// </summary>
    /// <remarks>
    /// Offline (<c>RID_*</c>) messages are not RakNet datagrams: RakNet sends them as a bare message,
    /// so the captured bytes <em>are</em> the message and there is no per-packet header to strip.
    /// </remarks>
    private static byte[] NewestRawTx(WrapperHarness harness, int txBefore)
    {
        IReadOnlyList<CaptureLine> tx = harness.Tx;
        Assert.Equal(txBefore + 1, tx.Count);
        return tx[^1].Bytes;
    }

    /// <summary>
    /// Returns the payload of the newest Tx datagram, asserting that exactly one new datagram was
    /// written since <paramref name="txBefore"/> and that it carries a single packet with the expected
    /// wire id.
    /// </summary>
    private static byte[] PayloadOfTheNewestTx(WrapperHarness harness, int txBefore, byte expectedWireId)
    {
        IReadOnlyList<CaptureLine> tx = harness.Tx;
        Assert.Equal(txBefore + 1, tx.Count);

        CaptureLine newest = tx[^1];
        Assert.True(
            RakNetDatagram.TryParse(newest.Bytes, newest.Bytes.Length, out RakNetDatagram datagram, out string error),
            error);

        RakNetPacketRecord packet = Assert.Single(datagram.Packets);
        Assert.Equal(expectedWireId, packet.Payload[0]);
        Assert.Equal(packet.Payload.Length, (packet.BitCount + 7) >> 3);

        return packet.Payload;
    }

    /// <summary>
    /// The hand-transcribed expected outcome of the full replay: one row per <c>PacketReceived</c>,
    /// in order, as <c>(capture.log record, 0-based mtaId, stripped payload hex)</c>.
    ///
    /// Sources: the <c>[pN wire=..]</c> tokens and payload bytes of each record, decoded against
    /// the mapping (the <c>-99</c> offset and the <c>4 ⇄ 0x13</c> swap) and the join-data strip. The
    /// same table is cross-checked mechanically by
    /// <see cref="EveryReplayedPayloadIsTheDatagramPayloadMinusItsWireId"/>.
    /// </summary>
    private static readonly (int Record, byte MtaId, string Payload)[] Replay =
    {
        (﻿6, 3, "3F574DA7EAC53F574DE1C9E200000000FFFF00000000FFFF00000000FFFF00000000FFFF00000000FFFF00000000FFFF00000000FFFF00000000FFFF00000000FFFF"),
        (12, 4, "DA01600191000F00312E362E302D392E32343134392E3085A9A0A2A99F2730B737A630B83A37B800000000000000000000000000000000000000000000000000000000000000000000000000000000000000078000006460A4151F2730B700"),
        (12, 25, "000000003F04757073740404000014000000000013000100000119000000000019000000010001000000010000000000010100000000000000000000B400C50096009500"),
        (16, 25, "0000000081C8047564646C060000000964622F75703434622F81B3656E633D310A352D3238814913006A6D656D6B6E626D69676A67626D6B6D6B6B6B77009AE0FEFB8AB0CCF190EDF6828AB8B1FC89F09D9DE8A5A891F5889496E4DED7EF3B2E2F3B00002F00193414151317373610173736391416193819111715341816151316383611161811383716383B3619131513131718171019101418181911391034103B361410361934141513173736171219173B11191815183B173417131711153B341416361616341817161413133B3633372F00193414151317373610173736391416193819111715341816151316383611161811383716383B36191537371339361613191014181819113937193739151811151934141513173736171219173B11191815183B173417131711153B3414163616163418171614131339353533009C000000000100007700EB938C8F83C4CB84909BF38583BAC18BFA8692EA9CA6D3E1F688E49597AAD59837373333050100000006000002003A273A371B0119393800002D2D2C3802786A0D666A6D6966676D69686A6D6264303638286F3414372D7F4C4792C0B8A3ABC5CED16ED2ADB3CADDDEB463C3B8D0DDB7CBA53E87F8E1EE8388E21B1A15111047444449"),
        (18, 24, "00"),
        (18, 24, "0300"),
        (20, 32, "0000000000180000000000000000000001006400008807D13B0000"),
        (23, 24, "01"),
        (82, 34, "0100000016C22A447ED70871F3E72E0133C5190CF09A360A3090440154484243"),
        (82, 34, "0100000014C22A447ED70871F3823B0128D90B17F8DF6A503809A42A7307000000005CE0BFB590E8396248424C4B00000000000000001C895A6C0D949A00"),
        (87, 32, "030000000018000000000000000080800100320076AA7806914B1CC8EE8000"),
        (87, 85, "01010000"),
        (89, 32, "03000000008800000000000001008E800100640001100FA2760000"),
        (91, 32, "03000000008800000000000001008E800100640001100FA2760000"),
        (93, 32, "03000000009800000000000001008E8001003200008807D13B0000"),
        (97, 32, "03000000008800000000000001008E800100640001100FA2760000"),
        (99, 85, "01010000"),
        (101, 32, "03000000008800000000000001008E800100640001100FA2760000"),
        (103, 32, "03000000008800000000000001008E800100640001100FA2760000"),
        (105, 32, "03000000009800000000000001008E8001003200008807D13B0000"),
        (109, 32, "03000000008800000000000001008E800100640001100FA2760000"),
        (111, 32, "03000000008800000000000001008E800100640001100FA2760000"),
        (113, 85, "01010000"),
        (115, 32, "03000000008800000000000001008E800100640001100FA2760000"),
        (117, 32, "03000000009800000000000001008E8001003200008807D13B0000"),
        (121, 32, "03000000008800000000000001008E800100640001100FA2760000"),
        (123, 32, "03000000008800000000000001008E800100640001100FA2760000"),
        (125, 31, "00002040200200"),
        (127, 32, "03000020408800000038000001008E800100640001100FA2760000"),
        (129, 85, "01010000"),
        (131, 32, "03000020409800000188000001008E8001016366063D3FFFE640001100FA276000"),
        (135, 31, "08002040200200"),
        (137, 32, "03080020408800000144020001008E800100640001100FA2760000"),
        (139, 34, "0100000014C22A447ED70871F3823B0128D90B17F8DF6A503809A52A7307000000005FF22CF29BE63AC048424C4B0000000000000000A3FB26E70A700828"),
        (143, 32, "03080020408800000108040001008E800100640001100FA2760000"),
        (145, 31, "081FE040200200"),
        (147, 32, "03081FE0408832000058080001008E8154E4640001100FA2760000"),
        (149, 32, "03081FC00099460200A40C0001008E8000C1D2E8F33D2076A640001100FA276000"),
        (153, 85, "01010000"),
        (155, 31, "001FC00A940200"),
        (157, 32, "03001FC00089B40601160E0001008E80AA8A640001100FA2760000"),
        (159, 31, "001FC00010BA00"),
        (161, 32, "03001FC000899C0C013C0E0001008E80347A6400F310EF82660000"),
        (163, 31, "001FC0118C1240"),
        (165, 31, "001FC02E0A0240"),
        (167, 32, "03001FC00088A812017E0C0001008E81A2486401AD33AEC2E40000"),
        (169, 31, "001FC001885A80"),
        (171, 31, "001FC02F862280"),
        (173, 32, "03001FC00098161601E4080001008E811A29A94DF93D28C526401E155CE4354000"),
        (177, 31, "00000038416AC0"),
        (179, 32, "030000000089F0160150040001008E802E1C64019B998DA3340000"),
        (181, 85, "01010000"),
        (183, 32, "030000000088EC180094020001008E802E1C6400F1BAED83240000"),
        (185, 32, "03000000008918180018020001008E802E1C6400ABDC8DA2940000"),
        (187, 32, "03000000009918180018020001008E802E1C32007DFF76F10A0000"),
        (191, 32, "03000000008918180018020001008E802E1C640111FF0DE2140000"),
        (193, 85, "01010000"),
        (195, 32, "03000000008918180018020001008E802E1C64012DFF0E62140000"),
        (197, 32, "03000000008918180018020001008E802E1C6401BBDDAFC2960000"),
        (199, 32, "03000000009918180018020001008E802E1C32000EEE07F18B0000"),
        (203, 32, "03000000008918180018020001008E802E1C640029BA2F43260000"),
        (205, 32, "03000000008918180018020001008E802E1C640127772F43460000"),
        (207, 85, "01010000"),
        (209, 32, "03000000008918180018020001008E802E1C6401E755EF43560000"),
        (211, 32, "03000000009918180018020001008E802E1C3200CCAAC7A1AB0000"),
        (215, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (217, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (219, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (221, 85, "01010000"),
        (223, 32, "03000000009918180018020001008E802E1C3200CCAAC7A1AB0000"),
        (227, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (229, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (231, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (233, 32, "03000000009918180018020001008E802E1C3200CCAAC7A1AB0000"),
        (233, 85, "01010000"),
        (237, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (241, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (243, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (245, 32, "03000000009918180018020001008E802E1C3200CCAAC7A1AB0000"),
        (249, 85, "01010000"),
        (251, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (253, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (255, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (257, 32, "03000000009918180018020001008E802E1C3200CCAAC7A1AB0000"),
        (261, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (263, 85, "01010000"),
        (265, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (267, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (269, 32, "03000000009918180018020001008E802E1C3200CCAAC7A1AB0000"),
        (273, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (275, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (277, 85, "01010000"),
        (279, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (281, 32, "03000000009918180018020001008E802E1C3200CCAAC7A1AB0000"),
        (285, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (287, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (289, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (291, 85, "01010000"),
        (293, 32, "03000000009918180018020001008E802E1C3200CCAAC7A1AB0000"),
        (297, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (299, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (301, 32, "03000000008918180018020001008E802E1C640199558F43560000"),
        (303, 5, ""),
    };
}
