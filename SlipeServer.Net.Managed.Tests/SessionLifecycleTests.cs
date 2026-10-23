using SlipeServer.Packets.Enums;
using Xunit;

using SlipeServer.Net.Managed;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Layer D: a session record must not outlive the peer that opened it.
///
/// <para>
/// <b>The defect this pins.</b> <c>MtaNetWrapper</c> keeps one <c>MtaNetSession</c> per RakNet binary
/// address in a <c>ConcurrentDictionary</c>, and before this contract existed the only thing that ever
/// removed an entry was <c>sessions.Clear()</c> inside <c>Stop()</c>. Neither
/// <c>RID_DISCONNECTION_NOTIFICATION</c> (0x13) nor <c>RID_CONNECTION_LOST</c> (0x14) retired the
/// record, so a peer that left stayed in the table forever. Two consequences follow, and both are
/// observable in the POC logs of the two-client run:
/// </para>
///
/// <para>
/// <b>1. A reconnect from the same address got silence.</b> <c>HandleOpenConnectionRequest</c> answers
/// an open request on a <c>HandshakeComplete</c> session with <c>"no reply"</c>, so a client whose
/// retry happened to reuse its previous source port was ignored — <c>logs/poc-20261018-132624.log</c>
/// line 2286 shows the one reconnect that succeeded arrived from a brand new port, which is a new
/// binary address and therefore a new record. <b>2. The slot count leaked.</b>
/// <c>HandleOpenConnectionRequest</c> refuses with <c>0x12</c> once <c>sessions.Count</c> reaches
/// <c>playerCount</c>, so every departed peer permanently consumed a slot.
/// </para>
///
/// <para>
/// <b>The contract.</b> A transport-level teardown retires the record, and it does so <i>after</i> the
/// MTA packet the native adapter synthesises has been delivered — Slipe still needs the quit, the
/// port just must not keep the address afterwards. A live session keeps the existing behaviour: an
/// open request against it is still answered with silence, because a peer that is already connected
/// must not have its session reset underneath it.
/// </para>
///
/// <para>
/// Every input here is an independent literal built from the documented framing — never the port's
/// own writer.
/// <see cref="TheTranscribedTeardownDatagramsParseAsDocumented"/> checks them against
/// <see cref="ReferenceDatagram"/>, an independent decoder, so a transcription slip cannot make the
/// behavioural tests pass vacuously. Outbound bytes are read back the same way.
/// </para>
/// </summary>
public class SessionLifecycleTests
{
    // Datagram layout, written out by hand:
    //   [hasAcks = 0][hasSecond = 0][sequence : 32][reliability = 3 : 3][channel = 0 : 5]
    //   [orderingIndex : 32][hasSplit = 0][compressed 16-bit bitCount = 8][pad to a byte][payload]
    // An 8-bit payload puts the count's compressed form ("1" + "0" + 8 bits) at bit 75 and the
    // payload at byte 11, so the datagram is 12 bytes.
    //
    // RakNet writes a full-width integer by reading it into a little-endian buffer and emitting each
    // byte MSB-first, which is why the sequence number and ordering index read as low byte first.
    // Reliability 3 (RELIABLE_ORDERED) is what the real client used for its own 0x13 in
    // capture-20261018-132624.log seq 404; that record carried oi=9, so ordering fields are present.

    /// <summary>
    /// <c>RID_DISCONNECTION_NOTIFICATION</c>: sequence 1, channel 0, ordering index 0, payload
    /// <c>0x13</c>. The sequence and ordering index are full-width integers, which RakNet writes by
    /// reading them into a little-endian buffer and emitting each byte MSB-first, so the 32-bit run
    /// reads back byte-reversed.
    /// </summary>
    private const string DisconnectionNotification = "0040000018000000001C0013";

    /// <summary><c>RID_CONNECTION_LOST</c>: sequence 2, channel 0, ordering index 0, payload <c>0x14</c>.</summary>
    private const string ConnectionLost = "0080000018000000001C0014";

    [Fact]
    public void TheTranscribedTeardownDatagramsParseAsDocumented()
    {
        foreach (string hex in new[] { DisconnectionNotification, ConnectionLost })
        {
            Assert.True(
                ReferenceDatagram.TryParse(WireTestKit.Hex(hex), out ReferenceParsedDatagram? parsed, out string error),
                error);

            ReferencePacket packet = Assert.Single(parsed!.Packets);
            Assert.Equal(3, packet.Reliability);
            Assert.Equal(0, packet.Channel);
            Assert.False(packet.HasSplit);
            Assert.Equal(8, packet.BitCount);

            byte payload = Assert.Single(packet.Payload);
            Assert.Equal(WireTestKit.Hex(hex)[^1], payload);
        }
    }

    /// <summary>
    /// The native adapter turns <c>0x13</c> into MTA id 5 (<c>PACKET_ID_PLAYER_QUIT</c>) with the
    /// system byte stripped, so Slipe sees an empty body — <c>logs/poc-20261018-132624.log</c> lines
    /// 334/337 are that pair. Retiring the record must not happen before the delivery.
    /// </summary>
    [Fact]
    public void ADisconnectionNotificationDeliversTheQuitAndThenRetiresTheSession()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        Assert.Equal(1, harness.Wrapper.SessionCount);

        harness.InjectRawHex(DisconnectionNotification);

        Received quit = Assert.Single(harness.Received);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_QUIT, quit.Id);
        Assert.Equal(harness.Address, quit.Address);
        Assert.Empty(quit.Data);

        Assert.Equal(0, harness.Wrapper.SessionCount);
    }

    /// <summary>
    /// <c>0x14</c> is the third adapter arm, not an unmapped id: <c>FUN_1003c3d0</c>'s case
    /// <c>0x14</c> calls <c>FUN_1003c470</c>, which logs <c>ConnectionLost %s</c> and raises MTA id 6
    /// (<c>PACKET_ID_PLAYER_TIMEOUT</c>) through the shared <c>FUN_10026910</c> — instruction for
    /// instruction the same shape as the <c>0x13</c> arm
    /// (<c>0x1003c568: PUSH 0x6</c> / <c>0x1003c56a: CALL 0x10026910</c>).
    ///
    /// <para>
    /// <c>0x14</c> does have an adapter mapping, and the assertion pins that direction.
    /// </para>
    ///
    /// <para>
    /// Delivery must precede removal, the same invariant the <c>0x13</c> test pins: the second
    /// <see cref="INetWrapper"/> handler records the session count <em>at the moment</em> the packet
    /// arrives, so a port that removed first and notified second would observe zero.
    /// </para>
    /// </summary>
    [Fact]
    public void ConnectionLostDeliversTheTimeoutAndThenRetiresTheSession()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        int sessionsAtDelivery = -1;
        harness.Wrapper.PacketReceived += (_, _, _, _, _) =>
            sessionsAtDelivery = harness.Wrapper.SessionCount;

        harness.InjectRawHex(ConnectionLost);

        Received timeout = Assert.Single(harness.Received);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_TIMEOUT, timeout.Id);
        Assert.Equal(harness.Address, timeout.Address);
        Assert.Empty(timeout.Data);

        Assert.Equal(1, sessionsAtDelivery);
        Assert.Equal(0, harness.Wrapper.SessionCount);
    }

    /// <summary>
    /// The end-to-end consequence, and the reason the fix exists: the same binary address can come
    /// back. The wrapper is built with a single slot so this also pins that the retired record stopped
    /// counting against <c>playerCount</c> — a leaked entry would make the open request hit the
    /// <c>0x12</c> refusal instead of being answered.
    /// </summary>
    [Fact]
    public void AReconnectFromTheRetiredAddressIsAnswered()
    {
        using var harness = new WrapperHarness(playerCount: 1);
        harness.ConnectSession();

        harness.InjectRawHex(DisconnectionNotification);
        Assert.Equal(0, harness.Wrapper.SessionCount);

        int before = harness.Tx.Count;
        harness.InjectHex(CaptureVectors.OpenConnectionRequest);

        Assert.Equal(1, harness.Wrapper.SessionCount);

        CaptureLine reply = Assert.Single(harness.Tx.Skip(before).ToList());

        // The handshake reply is an offline datagram, not a RakNet reliability frame, so it is read
        // as raw bytes: [0x0A][offline magic : 16][server GUID : 16] = 33 bytes.
        byte[] bytes = reply.Bytes;
        Assert.Equal(33, bytes.Length);
        Assert.Equal(0x0A, bytes[0]); // RID_OPEN_CONNECTION_REPLY
        Assert.Equal(
            new byte[]
            {
                0x00, 0xFF, 0x00, 0xFF, 0xFE, 0xFE, 0xFE, 0xFE,
                0xFD, 0xFD, 0xFD, 0xFD, 0x78, 0x56, 0x34, 0x17,
            },
            bytes[1..17]);
    }

    /// <summary>
    /// The other half of the contract, so the fix cannot be widened into "always answer": a live
    /// session still absorbs an open request silently. Resetting a connected peer's session would drop
    /// its negotiated bitstream version and its reliability state mid-session.
    /// </summary>
    [Fact]
    public void AnOpenConnectionRequestFromALiveSessionIsNotAnswered()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        int before = harness.Tx.Count;
        harness.InjectHex(CaptureVectors.OpenConnectionRequest);

        Assert.Equal(before, harness.Tx.Count);
        Assert.Equal(1, harness.Wrapper.SessionCount);
        Assert.Contains(harness.Trace, line => line.Contains("already-connected", StringComparison.Ordinal));
    }
}
