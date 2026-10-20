using System;
using System.Collections.Generic;
using System.Linq;
using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// The eight lossy-link scenarios the retransmission work exists to satisfy, driven through
/// <see cref="LossyLinkHarness"/>: two real wrappers, a seeded link, and a manual clock. No test here
/// sleeps, waits or starts a socket, so every one of them is a pure function of its own inputs.
/// </summary>
public class LossyLinkRetransmissionTests
{
    /// <summary>A one-byte game body. Any admitted wire id would do; this one is already the suite's.</summary>
    private static readonly byte[] Body = { 0x42 };

    // ------------------------------------------------------------------ 1. one drop

    /// <summary>
    /// The core case: a reliable packet is dropped once, the sender resends it on its own timeout,
    /// and the receiver delivers it exactly once. The resend must reuse the original sequence number
    /// and ordering index — anything else would be a new packet, not a repair.
    /// </summary>
    [Fact]
    public void ADroppedReliablePacketIsRetransmittedAndDeliveredExactlyOnce()
    {
        using var link = new LossyLinkHarness();

        uint dropped = 0;
        bool droppedOnce = false;
        link.Policy = datagram =>
        {
            if (droppedOnce || datagram.FromClient || datagram.Packets.Count != 1)
                return LinkAction.Pass;

            droppedOnce = true;
            dropped = datagram.Packets[0].Sequence;
            return LinkAction.Discard;
        };

        link.Send(PacketId.PACKET_ID_PLAYER_PURESYNC, Body);

        Assert.Empty(link.Client.Received);
        Assert.Equal(1, link.ServerSession().UnackedReliableCount);

        // The timeout is fixed, so the boundary is assertable: one millisecond short of it nothing is
        // owed, at it the resend goes out. No sleep — the clock is the test's.
        Assert.Equal(0, link.Advance(RetransmissionPolicy.TimeoutMilliseconds - 1));
        Assert.Empty(link.Client.Received);

        Assert.Equal(1, link.Advance(1));

        Received delivered = Assert.Single(link.Client.Received);
        Assert.Equal(Body, delivered.Data);
        Assert.Equal(0, link.ServerSession().UnackedReliableCount);

        // The resend is on the wire as a second datagram with the same sequence number, the same
        // ordering index and the same payload, and it carries no ack block: the acks were consumed by
        // the first transmission and re-attaching them would duplicate them.
        IReadOnlyList<CaptureLine> tx = link.ServerTx;
        Assert.Equal(2, tx.Count);

        ReferenceParsedDatagram first = Lossy(tx[0]);
        ReferenceParsedDatagram again = Lossy(tx[1]);

        ReferencePacket original = Assert.Single(first.Packets);
        ReferencePacket resent = Assert.Single(again.Packets);

        Assert.Equal(dropped, original.Sequence);
        Assert.Equal(original.Sequence, resent.Sequence);
        Assert.Equal(original.OrderingIndex, resent.OrderingIndex);
        Assert.Equal(original.Channel, resent.Channel);
        Assert.Equal(original.Payload, resent.Payload);
        Assert.Empty(again.Acks);

        Assert.Contains(
            link.Server.Trace,
            line => line.Contains($"retransmit seq={dropped} attempt 1/", StringComparison.Ordinal));
    }

    // ------------------------------------------------------------------ 2. two drops

    /// <summary>
    /// Losing the first resend as well must not end the story: the ledger keeps the packet until its
    /// attempt budget is spent, so a later attempt still delivers it.
    /// </summary>
    [Fact]
    public void ADroppedFirstRetransmissionIsRetransmittedAgain()
    {
        using var link = new LossyLinkHarness();

        int dropsLeft = 2;
        link.Policy = datagram =>
        {
            if (dropsLeft == 0 || datagram.FromClient || datagram.Packets.Count != 1)
                return LinkAction.Pass;

            dropsLeft--;
            return LinkAction.Discard;
        };

        link.Send(PacketId.PACKET_ID_PLAYER_PURESYNC, Body);
        Assert.Empty(link.Client.Received);

        Assert.Equal(1, link.Advance(RetransmissionPolicy.TimeoutMilliseconds));
        Assert.Empty(link.Client.Received);

        Assert.Equal(1, link.Advance(RetransmissionPolicy.TimeoutMilliseconds));

        Assert.Equal(Body, Assert.Single(link.Client.Received).Data);

        IReadOnlyList<CaptureLine> tx = link.ServerTx;
        Assert.Equal(3, tx.Count);
        Assert.Single(tx.Select(line => Assert.Single(Lossy(line).Packets).Sequence).Distinct());
        Assert.Contains(
            link.Server.Trace,
            line => line.Contains("retransmit seq=", StringComparison.Ordinal) &&
                    line.Contains("attempt 2/", StringComparison.Ordinal));
    }

    // ------------------------------------------------------------------ 3. unreliable

    /// <summary>
    /// An unreliable packet has no retransmission contract, so a lost one stays lost: it is never
    /// entered in the ledger and no amount of elapsed time produces a resend.
    /// </summary>
    [Fact]
    public void AnUnreliablePacketIsNeverRetransmitted()
    {
        using var link = new LossyLinkHarness();
        link.Policy = datagram => datagram.FromClient ? LinkAction.Pass : LinkAction.Discard;

        link.Send(
            PacketId.PACKET_ID_PLAYER_PURESYNC,
            Body,
            reliability: PacketReliability.Unreliable);

        Assert.Single(link.ServerTx);
        Assert.Empty(link.Client.Received);
        Assert.Equal(0, link.ServerSession().UnackedReliableCount);

        // Six timeout windows later the link is still silent.
        Assert.Equal(0, link.Advance(6 * RetransmissionPolicy.TimeoutMilliseconds));

        Assert.Single(link.ServerTx);
        Assert.Empty(link.Client.Received);
        Assert.DoesNotContain(link.Server.Trace, line => line.Contains("retransmit", StringComparison.Ordinal));
    }

    // ------------------------------------------------------------------ 4. duplication

    /// <summary>
    /// Duplicating every datagram must not duplicate the application's view. The reliability layer
    /// dedupes on sequence number, re-acks the copy, and delivers once.
    /// </summary>
    [Fact]
    public void ADuplicatedDatagramIsDeliveredOnce()
    {
        using var link = new LossyLinkHarness();
        link.Policy = _ => LinkAction.Duplicate(3);

        link.Send(PacketId.PACKET_ID_PLAYER_PURESYNC, Body);

        Received delivered = Assert.Single(link.Client.Received);
        Assert.Equal(Body, delivered.Data);

        Assert.Contains(link.Client.Trace, line => line.Contains("duplicate seq", StringComparison.Ordinal));

        // Every copy still earns an ack, so the sender's ledger drains: a duplicate is not a loss.
        Assert.Equal(0, link.ServerSession().UnackedReliableCount);
    }

    // ------------------------------------------------------------------ 5. reordering

    /// <summary>
    /// Delivering the second packet before the first must still produce ordering-index order at the
    /// application. The ordering layer holds the overtaker behind the hole and releases both when the
    /// hole fills.
    /// </summary>
    [Fact]
    public void ReorderedPacketsAreDeliveredInOrderingIndexOrder()
    {
        using var link = new LossyLinkHarness();

        link.Policy = datagram =>
            !datagram.FromClient &&
            datagram.Packets.Count == 1 &&
            datagram.Packets[0].OrderingIndex == 0
                ? LinkAction.Hold(50)
                : LinkAction.Pass;

        link.Send(PacketId.PACKET_ID_PLAYER_PURESYNC, new byte[] { 0x01 });
        link.Send(PacketId.PACKET_ID_PLAYER_PURESYNC, new byte[] { 0x02 });

        // The second packet overtook the first, so nothing has been released yet.
        Assert.Empty(link.Client.Received);
        Assert.Contains(link.Client.Trace, line => line.Contains("held ordered ch=0 oi=1", StringComparison.Ordinal));

        link.Advance(50);

        Assert.Equal(new byte[] { 0x01, 0x02 }, link.Client.Received.Select(received => received.Data[0]));

        // Holding a packet for 50 ms is far inside the 500 ms timeout, so nothing was resent.
        Assert.Equal(2, link.ServerTx.Count);
        Assert.DoesNotContain(link.Server.Trace, line => line.Contains("retransmit", StringComparison.Ordinal));
    }

    // ------------------------------------------------------------------ 6. split parts

    /// <summary>
    /// A split message is retained part by part, so losing one part resends one part — not the whole
    /// message — and the message reassembles only once that part arrives.
    /// </summary>
    [Fact]
    public void ADroppedSplitPartIsRetransmittedAndTheMessageReassemblesOnlyThen()
    {
        using var link = new LossyLinkHarness();

        var body = new byte[4000];
        for (int i = 0; i < body.Length; i++)
            body[i] = (byte)i;

        bool dropped = false;
        link.Policy = datagram =>
        {
            if (dropped || datagram.FromClient)
                return LinkAction.Pass;

            foreach (ReferencePacket packet in datagram.Packets)
            {
                if (!packet.HasSplit || packet.SplitIndex != 1)
                    continue;

                dropped = true;
                return LinkAction.Discard;
            }

            return LinkAction.Pass;
        };

        link.Send(PacketId.PACKET_ID_PLAYER_PURESYNC, body);

        Assert.True(dropped, "the payload must split for this test to mean anything");

        // Every other part arrived and was buffered; the message cannot complete without part 1.
        Assert.Empty(link.Client.Received);
        Assert.DoesNotContain(link.Client.Trace, line => line.Contains("reassembled", StringComparison.Ordinal));
        Assert.Contains(link.Client.Trace, line => line.Contains("buffered split part", StringComparison.Ordinal));

        // Exactly one part is owed: the other parts were acknowledged and released.
        Assert.Equal(1, link.ServerSession().UnackedReliableCount);

        link.Advance(RetransmissionPolicy.TimeoutMilliseconds);

        Received delivered = Assert.Single(link.Client.Received);
        Assert.Equal(body, delivered.Data);
        Assert.Contains(link.Client.Trace, line => line.Contains("reassembled sid=", StringComparison.Ordinal));

        // The resend was one part, not the whole message.
        ReferencePacket resent = Lossy(link.ServerTx[^1]).Packets.Single();
        Assert.True(resent.HasSplit);
        Assert.Equal(1u, resent.SplitIndex);
        Assert.Equal(0, link.ServerSession().UnackedReliableCount);
    }

    // ------------------------------------------------------------------ 7. blackhole

    /// <summary>
    /// A link that never delivers anything must not spin forever. The attempt count is bounded, the
    /// give-up is named on the trace, and the session's retained state is released rather than leaked.
    /// </summary>
    [Fact]
    public void APermanentBlackholeGivesUpAfterTheBoundedAttemptCount()
    {
        using var link = new LossyLinkHarness();
        link.Policy = _ => LinkAction.Discard;

        link.Send(PacketId.PACKET_ID_PLAYER_PURESYNC, Body);

        MtaNetSession session = link.ServerSession();
        Assert.Equal(1, session.UnackedReliableCount);

        int resends = 0;
        for (int i = 0; i <= RetransmissionPolicy.MaximumRetransmissions; i++)
            resends += link.Advance(RetransmissionPolicy.TimeoutMilliseconds);

        Assert.Equal(RetransmissionPolicy.MaximumRetransmissions, resends);

        // Bounded, then abandoned — and the give-up is legible in the log.
        Assert.Contains(
            link.Server.Trace,
            line => line.Contains("retransmit give-up: seq=", StringComparison.Ordinal));
        Assert.Contains(link.Server.Trace, line => line.Contains("retiring the session", StringComparison.Ordinal));

        // The give-up also notifies the game layer, exactly once, and does so before the record goes.
        Received timeout = Assert.Single(link.Server.Received);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_TIMEOUT, timeout.Id);
        Assert.Equal(link.ServerAddress, timeout.Address);
        Assert.Empty(timeout.Data);

        // Nothing is retained for a peer that is gone.
        Assert.Equal(0, link.Server.Wrapper.SessionCount);
        Assert.Equal(0, session.UnackedReliableCount);

        // And the timer has nothing left to do: further time produces no further traffic.
        Assert.Equal(0, link.Advance(10 * RetransmissionPolicy.TimeoutMilliseconds));
        Assert.Equal(RetransmissionPolicy.MaximumRetransmissions + 1, link.ServerTx.Count);
        Assert.Empty(link.Client.Received);
    }

    // ------------------------------------------------------------------ 8. stray ack

    /// <summary>
    /// An ack for a sequence number this end never sent is not an error and must not disturb the
    /// ledger. Native logs <c>ERR unmatchedAcks:%d</c> and carries on; the packet genuinely in flight
    /// stays in flight.
    /// </summary>
    /// <remarks>
    /// The datagram is a hand-written literal, not something the port produced: has-acks set, a zero
    /// timestamp, one single-sequence range for 999, and no packets.
    /// </remarks>
    [Fact]
    public void AnAckForASequenceNumberThatWasNeverSentIsIgnoredHarmlessly()
    {
        const string AckForUnsentSequence = "800000000000000063E7030000";

        // The literal really does acknowledge 999 and nothing else.
        Assert.True(ReferenceDatagram.TryParse(
            WireTestKit.Hex(AckForUnsentSequence), out ReferenceParsedDatagram? literal, out string error), error);
        Assert.Equal((999u, 999u), Assert.Single(literal!.Acks));
        Assert.Empty(literal.Packets);

        using var link = new LossyLinkHarness();

        bool blackholed = true;
        link.Policy = datagram => blackholed && !datagram.FromClient
            ? LinkAction.Discard
            : LinkAction.Pass;

        link.Send(PacketId.PACKET_ID_PLAYER_PURESYNC, Body);

        MtaNetSession session = link.ServerSession();
        uint inFlight = session.UnackedReliable.Single().Key;
        Assert.Equal(1, session.UnackedReliableCount);

        // A bogus ack that covers a wide range of real sequence numbers still clears nothing it did
        // not send: the ledger is walked, not the range.
        link.Server.InjectRawHex(AckForUnsentSequence, LossyLinkHarness.ClientEndPoint);

        Assert.Equal(1, link.Server.Wrapper.SessionCount);
        Assert.Equal(1, session.UnackedReliableCount);
        Assert.Contains(session.UnackedReliable, entry => entry.Key == inFlight);

        // The link recovers, so the ignore was harmless rather than merely quiet.
        blackholed = false;
        link.Advance(RetransmissionPolicy.TimeoutMilliseconds);

        Assert.Equal(Body, Assert.Single(link.Client.Received).Data);
        Assert.Equal(0, session.UnackedReliableCount);
    }

    private static ReferenceParsedDatagram Lossy(CaptureLine line) => LossyLinkHarness.Parse(line);
}
