using SlipeServer.Packets.Enums;
using Xunit;

using SlipeServer.Net.Managed;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Layer D: the two behavioural invariants that a refactor is most likely to break silently.
///
/// <para>
/// <b>1. A rejection is a silent drop.</b> A rejected packet produces no reply and no disconnect. The one thing the port <i>does</i> emit is the reliability-layer ack, which is queued
/// before any gate runs — so "silent" is asserted as "the only thing on the wire is an ack-only
/// datagram carrying zero packets".
/// </para>
///
/// <para>
/// <b>2. Acks are generated, not repeated.</b> Every accepted packet earns an ack; the queue is
/// cleared on flush; a datagram that carries only the peer's acks produces nothing back.
/// </para>
///
/// <para>
/// Every input here is a literal transcribed from <c>capture.log</c> or from the documented datagram
/// layout — never the port's own
/// writer. Outbound bytes are read back with <see cref="ReferenceDatagram"/>, an independent decoder
/// that shares no code with <c>BitStream</c>, <c>RakNetDatagram</c> or <c>RakNetDatagramWriter</c>,
/// so agreement is evidence rather than a self-round-trip.
/// </para>
/// </summary>
public class SilentDropAndAckTests
{
    /// <summary>Three zero bytes: below the 8-byte game-path floor.</summary>
    private const string SubFloorDatagram = "010000";

    // capture.log seq 8, RX, real client. One packet, wire id 0x00 — a RakNet system id with no
    // handler in this port, so it must be dropped silently (MtaNetWrapper.HandleRakNetSystemPacket's
    // default arm). 23 bytes: 0x61 header byte, 8-byte second field, then the packet.
    private const string RecordedSystemPacketIdZero =
        "61D00CC00000000000C000001248006840330000000000";

    // capture.log seq 6, RX, real client. Two packets with sequence numbers 1 and 2, both carrying a
    // wire id in the MTA band. Used for the ack-generation tests.
    private const string RecordedTwoPacketDatagram =
        "5A100CC000000000004000001800000000018020113F574DA7EAC53F574DE1C9" +
        "E200000000FFFF00000000FFFF00000000FFFF00000000FFFF00000000FFFF00" +
        "000000FFFF00000000FFFF00000000FFFF00000000FFFF020000004920004D40" +
        "330000000000";

    // capture.log seq 18, RX, real client. Two packets, both wire id 0x7B = mtaId 24.
    private const string RecordedTwoMtaId24Packets =
        "5C518CC00000000001C00000180080000010807B0008000000600300000043007B0300";

    // capture.log seq 15, RX, real client. Ack-only: the peer acknowledges sequences 2..2 and sends
    // no packet. 13 bytes.
    private const string RecordedAckOnlyDatagram = "80000000000000006302000000";

    /// <summary>
    /// <c>DoPulse</c> row 1: a datagram shorter than 8 bytes is discarded before anything inside it
    /// is looked at, so there is nothing to ack and nothing to reply with.
    /// </summary>
    [Fact]
    public void ASubFloorDatagramProducesNoEventAndNothingOnTheWire()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        int txBefore = harness.Tx.Count;

        harness.InjectHex(SubFloorDatagram);

        Assert.Empty(harness.Received);
        Assert.Equal(txBefore, harness.Tx.Count);
        Assert.Equal(1, harness.Wrapper.SessionCount);
    }

    /// <summary>
    /// The recorded <c>RID_OPEN_CONNECTION_REQUEST</c> is a bare message, not a RakNet datagram, so
    /// handing it to an established peer's game path fails the framing parse. A parse failure is a
    /// drop: no event, no reply.
    /// </summary>
    [Fact]
    public void AnUnparseableConnectedDatagramProducesNoEventAndNothingOnTheWire()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        int txBefore = harness.Tx.Count;

        harness.Inject(CaptureVectors.OpenRequestBytes);

        Assert.Empty(harness.Received);
        Assert.Equal(txBefore, harness.Tx.Count);
        Assert.Equal(1, harness.Wrapper.SessionCount);
    }

    /// <summary>
    /// A recorded packet whose wire id has no handler is dropped by the system-packet dispatcher. The
    /// only bytes written back are the reliability ack — a datagram carrying <b>zero</b> packets, not
    /// a reply.
    /// </summary>
    [Fact]
    public void ARecordedSystemPacketWithNoHandlerIsDroppedAndOnlyAcked()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        int txBefore = harness.Tx.Count;

        harness.InjectHex(RecordedSystemPacketIdZero);

        Assert.Empty(harness.Received);
        Assert.Single(harness.Tx.Skip(txBefore));
        AssertAckOnlyDatagram(harness.Tx[^1].Bytes);
        Assert.Equal(1, harness.Wrapper.SessionCount);
    }

    /// <summary>
    /// Gate 2 latches on the first packet that exceeds a configured budget and then rejects
    /// everything from that player. The second packet in the recorded datagram is therefore dropped
    /// even though the first was delivered, and the drop writes no reply.
    /// </summary>
    [Fact]
    public void AGateTwoRejectionDropsThePacketAndWritesNoReply()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        // One packet of mtaId 24 is allowed; the second trips the usage limit and latches.
        harness.Wrapper.AdmissionOptions.Gate2UsageLimits[24] = 1;

        int txBefore = harness.Tx.Count;

        harness.InjectHex(RecordedTwoMtaId24Packets);

        Received delivered = Assert.Single(harness.Received);
        Assert.Equal(PacketId.PACKET_ID_RPC, delivered.Id);

        Assert.Single(harness.Tx.Skip(txBefore));
        AssertAckOnlyDatagram(harness.Tx[^1].Bytes);
        Assert.Equal(1, harness.Wrapper.SessionCount);
    }

    /// <summary>
    /// Every packet in the recorded datagram earns an ack, and the port writes them back as one
    /// single-sequence range per packet. Native RakNet merges consecutive sequences into one range;
    /// this port does not, which is a byte-level difference from the recorded server's Tx stream.
    /// Both encodings are legal — the point of the
    /// assertion is to pin which one this port emits.
    /// </summary>
    [Fact]
    public void EveryAcceptedPacketEarnsItsOwnSingleSequenceAckRange()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        int txBefore = harness.Tx.Count;

        harness.InjectHex(RecordedTwoPacketDatagram);

        // The datagram carries two packets, but the second is a RakNet system packet (wire 0x00), so
        // only the mtaId-3 packet is delivered. Both are still acknowledged.
        Received delivered = Assert.Single(harness.Received);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_JOIN, delivered.Id);

        Assert.Single(harness.Tx.Skip(txBefore));
        ReferenceParsedDatagram ack = ParseReference(harness.Tx[^1].Bytes);

        Assert.Empty(ack.Packets);
        Assert.Equal(new[] { (1u, 1u), (2u, 2u) }, ack.Acks.ToArray());
    }

    /// <summary>
    /// <c>FlushAcks</c> clears the queue, so a peer datagram that carries nothing but its own acks
    /// produces nothing back. Without the clear, the port would re-acknowledge the same sequences
    /// forever.
    /// </summary>
    [Fact]
    public void AnAckOnlyDatagramFromThePeerProducesNoReply()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        int txBefore = harness.Tx.Count;

        harness.InjectHex(RecordedTwoPacketDatagram);
        int afterGamePacket = harness.Tx.Count;
        Assert.Equal(txBefore + 1, afterGamePacket);

        harness.InjectHex(RecordedAckOnlyDatagram);

        Assert.Equal(afterGamePacket, harness.Tx.Count);
        Assert.Single(harness.Received);
    }

    /// <summary>
    /// The documented invariant over the whole recording: replaying every Rx datagram of a real
    /// client session never makes the port emit a disconnect and never tears the session down. This
    /// is the assertion that would catch a refactor that "helpfully" started disconnecting on a
    /// rejected packet.
    /// </summary>
    [Fact]
    public void NoRecordedInboundDatagramEverProducesADisconnect()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        foreach (CaptureLine record in harness.Rx)
        {
            harness.Inject(record.Bytes);

            Assert.Equal(1, harness.Wrapper.SessionCount);
        }

        foreach (CaptureLine tx in harness.Tx)
        {
            Assert.DoesNotContain(tx.Packets, token => token.WireId == MtaProtocol.Rid.DisconnectionNotification);
        }
    }

    private static void AssertAckOnlyDatagram(byte[] datagram)
    {
        ReferenceParsedDatagram parsed = ParseReference(datagram);

        Assert.Empty(parsed.Packets);
        Assert.NotEmpty(parsed.Acks);
    }

    private static ReferenceParsedDatagram ParseReference(byte[] datagram)
    {
        Assert.True(ReferenceDatagram.TryParse(datagram, out ReferenceParsedDatagram? parsed, out string error), error);
        return parsed!;
    }
}
