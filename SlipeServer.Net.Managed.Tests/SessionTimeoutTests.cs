using System;
using System.Collections.Generic;
using System.Linq;
using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Layer D, the half an inbound handler cannot supply: the server deciding by itself that a peer is
/// gone.
///
/// <para>
/// <b>Why this cannot be an inbound arm.</b> <c>RID_CONNECTION_LOST</c> (0x14) is normally
/// <em>fabricated locally</em>: RakNet's update cycle notices that a peer's last received datagram has
/// aged past the timeout and synthesises the event, which is why a dead peer sends nothing at all. The
/// native shape is <c>CNetServer::DoPulse</c> -> <c>FUN_100691b0</c> (a scan, throttled to at most one
/// per second per peer by <c>999 &lt; elapsed</c>) -> <c>FUN_1006df40</c> (idle seconds against the
/// peer's threshold) -> <c>FUN_10068600</c> (close), and the resulting 0x14 then reaches the adapter
/// <c>FUN_1003c3d0</c> and becomes mtaId 6. Without the scan, a silent peer is never retired and Slipe
/// keeps its <c>IClient</c> in-game forever.
/// </para>
///
/// <para>
/// <b>Determinism.</b> Every test here drives <see cref="MtaNetWrapper.TickTimeouts"/> directly
/// against a <see cref="ManualClock"/>, exactly as the retransmission tests drive
/// <see cref="MtaNetWrapper.TickRetransmissions"/>. Nothing sleeps, nothing reads the wall clock, and
/// no socket is opened, so the timeout boundary is assertable to the millisecond in both directions.
/// </para>
/// </summary>
public class SessionTimeoutTests
{
    /// <summary>
    /// A four-byte datagram that no gate accepts. Its first byte <c>0x7F</c> is not an offline route id
    /// and has no magic geometry, so with a session record present it falls through to the connected
    /// path, where <c>HandleConnected</c>'s 8-byte game-path floor drops it
    /// (<c>MtaAdmission.MinimumDatagramBytes</c>). It therefore exercises the "a datagram arrived but no
    /// packet was delivered" case.
    /// </summary>
    private const string DroppedByTheFloor = "7F000000";

    private static (WrapperHarness Harness, ManualClock Clock) Build(ManualClock? clock = null)
    {
        clock ??= new ManualClock();
        return (new WrapperHarness(clock: clock), clock);
    }

    // ---------------------------------------------------------------- the boundary

    /// <summary>
    /// One millisecond short of the policy window nothing happens: the peer may still be alive, and
    /// retiring it early would drop a player over a GC pause or an alt-tab.
    /// </summary>
    [Fact]
    public void ASessionOneMillisecondShortOfTheTimeoutIsKept()
    {
        (WrapperHarness harness, ManualClock clock) = Build();
        using (harness)
        {
            harness.ConnectSession();

            clock.Advance(SessionTimeoutPolicy.TimeoutMilliseconds - 1);

            Assert.Equal(0, harness.Wrapper.TickTimeouts());
            Assert.Equal(1, harness.Wrapper.SessionCount);
            Assert.Empty(harness.Received);
        }
    }

    /// <summary>
    /// At exactly the policy window the peer is gone. The comparison is <c>elapsed &gt;= timeout</c>,
    /// matching the "at the timeout it fires" direction of the retransmission boundary tests.
    /// </summary>
    [Fact]
    public void ASessionAtTheTimeoutIsRetired()
    {
        (WrapperHarness harness, ManualClock clock) = Build();
        using (harness)
        {
            harness.ConnectSession();

            clock.Advance(SessionTimeoutPolicy.TimeoutMilliseconds);

            Assert.Equal(1, harness.Wrapper.TickTimeouts());
            Assert.Equal(0, harness.Wrapper.SessionCount);
        }
    }

    // ---------------------------------------------------------------- liveness

    /// <summary>
    /// A peer that keeps sending outlives the window indefinitely: each datagram resets the clock the
    /// sweep measures from. The stream is the recorded client game packet
    /// (<c>capture.log</c> seq 20, wire <c>0x83</c> -> mtaId 32), re-injected once per tick; its
    /// sequence number is already known to the reliability layer, so it is delivered once and
    /// acknowledged thereafter, which is exactly the point — liveness is "a datagram arrived", not
    /// "a packet was delivered".
    /// </summary>
    [Fact]
    public void APeerThatKeepsSendingOutlivesTheTimeout()
    {
        (WrapperHarness harness, ManualClock clock) = Build();
        using (harness)
        {
            harness.ConnectSession();

            for (int tick = 0; tick < 30; tick++)
            {
                clock.Advance(SessionTimeoutPolicy.TickMilliseconds);
                harness.InjectHex(CaptureVectors.RpcPacketWire83);

                Assert.Equal(0, harness.Wrapper.TickTimeouts());
                Assert.Equal(1, harness.Wrapper.SessionCount);
            }
        }
    }

    /// <summary>
    /// The stamp happens before any gate, so a peer whose datagrams are all being dropped is still
    /// alive. This is the case a naive "mark on delivery" implementation gets wrong: a client behind a
    /// broken gate would be retired even though its traffic is arriving.
    /// </summary>
    [Fact]
    public void APeerWhoseDatagramsAreDroppedByAGateStillCountsAsAlive()
    {
        (WrapperHarness harness, ManualClock clock) = Build();
        using (harness)
        {
            harness.ConnectSession();

            for (int tick = 0; tick < 30; tick++)
            {
                clock.Advance(SessionTimeoutPolicy.TickMilliseconds);
                harness.InjectRawHex(DroppedByTheFloor);

                Assert.Equal(0, harness.Wrapper.TickTimeouts());
                Assert.Equal(1, harness.Wrapper.SessionCount);
            }

            // The gate really did drop them: nothing reached the game layer and the floor is why.
            Assert.Empty(harness.Received);
            Assert.Contains(
                harness.Trace,
                line => line.Contains("below the 8-byte game-path floor", StringComparison.Ordinal));

            // ... and the peer was never retired, because it never went quiet.
            Assert.DoesNotContain(
                harness.Trace,
                line => line.Contains("silent for", StringComparison.Ordinal));
        }
    }

    // ---------------------------------------------------------------- the notification

    /// <summary>
    /// The whole point: the game layer hears <c>PACKET_ID_PLAYER_TIMEOUT</c>, exactly once, while the
    /// binary address is still meaningful. The second handler records the session count at the instant
    /// of delivery, so a port that retired first and notified second would see zero here.
    /// </summary>
    [Fact]
    public void TheTimeoutNotifiesOnceBeforeRetiringAndNeverTwice()
    {
        (WrapperHarness harness, ManualClock clock) = Build();
        using (harness)
        {
            harness.ConnectSession();

            var sessionsAtDelivery = new List<int>();
            harness.Wrapper.PacketReceived += (_, _, _, _, _) =>
                sessionsAtDelivery.Add(harness.Wrapper.SessionCount);

            clock.Advance(SessionTimeoutPolicy.TimeoutMilliseconds);

            Assert.Equal(1, harness.Wrapper.TickTimeouts());
            Assert.Equal(0, harness.Wrapper.SessionCount);

            // Later ticks over an empty table must not re-notify.
            Assert.Equal(0, harness.Wrapper.TickTimeouts());
            clock.Advance(SessionTimeoutPolicy.TimeoutMilliseconds);
            Assert.Equal(0, harness.Wrapper.TickTimeouts());

            Received timeout = Assert.Single(harness.Received);
            Assert.Equal(PacketId.PACKET_ID_PLAYER_TIMEOUT, timeout.Id);
            Assert.Equal(harness.Address, timeout.Address);
            Assert.Empty(timeout.Data);

            Assert.Equal(new[] { 1 }, sessionsAtDelivery);
        }
    }

    /// <summary>
    /// End to end from raw bytes: a real connected-session datagram stream, then silence, then a clock
    /// advance. The assertion is the one Slipe's handler depends on — the packet arrives in
    /// <see cref="INetWrapper"/> with the peer's binary address, and the record is gone afterwards so
    /// the address can be reused.
    /// </summary>
    [Fact]
    public void ASilentPeerIsRetiredEndToEndFromRawBytes()
    {
        (WrapperHarness harness, ManualClock clock) = Build();
        using (harness)
        {
            harness.ConnectSession();
            harness.InjectHex(CaptureVectors.RpcPacketWire83);

            Assert.Equal(1, harness.Wrapper.SessionCount);
            Assert.NotEmpty(harness.Received);

            harness.Received.Clear();
            int txBefore = harness.Tx.Count;

            clock.Advance(SessionTimeoutPolicy.TimeoutMilliseconds);
            Assert.Equal(1, harness.Wrapper.TickTimeouts());

            Received timeout = Assert.Single(harness.Received);
            Assert.Equal(PacketId.PACKET_ID_PLAYER_TIMEOUT, timeout.Id);
            Assert.Equal(harness.Address, timeout.Address);
            Assert.Equal(harness.Wrapper, timeout.Sender);

            Assert.Null(harness.Wrapper.FindSession(harness.Address));
            Assert.Equal(0, harness.Wrapper.SessionCount);

            // Nothing was put on the wire: the peer is presumed gone, so there is no one to tell.
            Assert.Equal(txBefore, harness.Tx.Count);
        }
    }

    /// <summary>
    /// The one-shot claim in <see cref="MtaNetSession.TryClaimRetirement"/> is what stops a second
    /// notification: a teardown that arrives after the sweep finds no record at all, because the
    /// sweep already removed it.
    /// </summary>
    [Fact]
    public void AnInboundTeardownAfterTheTimeoutNotifiesNothingFurther()
    {
        (WrapperHarness harness, ManualClock clock) = Build();
        using (harness)
        {
            harness.ConnectSession();

            clock.Advance(SessionTimeoutPolicy.TimeoutMilliseconds);
            Assert.Equal(1, harness.Wrapper.TickTimeouts());

            Received timeout = Assert.Single(harness.Received);

            // The client's own 0x13 arrives late, after the sweep already retired it.
            harness.InjectRawHex(CaptureVectors.DisconnectionNotification);

            Assert.Same(timeout, Assert.Single(harness.Received));
            Assert.Equal(0, harness.Wrapper.SessionCount);
        }
    }

    // ---------------------------------------------------------------- interaction with reliability

    /// <summary>
    /// A timed-out peer must stay dead. The retransmission ledger is released by
    /// <c>RemoveSession</c>, so the timer has nothing left to resend and cannot resurrect the record —
    /// the failure mode this pins is a sweep that retired the session while leaving its unacked
    /// packets behind, which would keep a dead endpoint in the send path forever.
    /// </summary>
    [Fact]
    public void TheRetransmissionTickDoesNotResurrectATimedOutPeer()
    {
        (WrapperHarness harness, ManualClock clock) = Build();
        using (harness)
        {
            harness.ConnectSession();

            MtaNetSession live = harness.Wrapper.FindSession(harness.Address)!;
            int owedBefore = live.UnackedReliableCount;

            harness.Wrapper.SendPacket(
                harness.Address,
                PacketId.PACKET_ID_PLAYER_PURESYNC,
                0x90,
                new byte[] { 0x42 },
                reliability: PacketReliability.Reliable);

            // One more packet is owed, so the timer genuinely has work it could do.
            Assert.Equal(owedBefore + 1, live.UnackedReliableCount);

            clock.Advance(SessionTimeoutPolicy.TimeoutMilliseconds);
            Assert.Equal(1, harness.Wrapper.TickTimeouts());
            Assert.Equal(0, harness.Wrapper.SessionCount);

            int txBefore = harness.Tx.Count;
            harness.Received.Clear();

            // Six retransmission windows later the timer still owes nothing: the ledger went with the
            // record, so there is no peer left to resend to and no packet left to resend.
            for (int tick = 0; tick < 6; tick++)
            {
                clock.Advance(RetransmissionPolicy.TimeoutMilliseconds);
                Assert.Equal(0, harness.Wrapper.TickRetransmissions());
            }

            Assert.Equal(txBefore, harness.Tx.Count);
            Assert.Empty(harness.Received);
            Assert.Equal(0, harness.Wrapper.SessionCount);

            // And the send path no longer knows the address, so nothing can re-create the record.
            harness.Wrapper.SendPacket(
                harness.Address,
                PacketId.PACKET_ID_PLAYER_PURESYNC,
                0x90,
                new byte[] { 0x42 },
                reliability: PacketReliability.Reliable);

            Assert.Equal(0, harness.Wrapper.SessionCount);
            Assert.Contains(
                harness.Trace,
                line => line.Contains("for unknown peer", StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// The sweep scans every record, so a peer that goes quiet is retired without touching one that is
    /// still talking. Pins that the tick is per-session, not "first silent peer wins".
    /// </summary>
    [Fact]
    public void OnlyTheSilentPeerIsRetired()
    {
        var clock = new ManualClock();
        using var harness = new WrapperHarness(clock: clock);

        var silent = WireTestKit.Endpoint("192.0.2.10:58057");
        var chatty = WireTestKit.Endpoint("192.0.2.40:58058");

        harness.ConnectSession(silent);
        harness.ConnectSession(chatty);

        Assert.Equal(2, harness.Wrapper.SessionCount);

        ulong silentAddress = MtaNetWrapper.BinaryAddressOf(silent);
        ulong chattyAddress = MtaNetWrapper.BinaryAddressOf(chatty);

        for (int tick = 0; tick < 15; tick++)
        {
            clock.Advance(SessionTimeoutPolicy.TickMilliseconds);
            harness.InjectHex(CaptureVectors.RpcPacketWire83, chatty);
            harness.Wrapper.TickTimeouts();
        }

        Assert.Equal(1, harness.Wrapper.SessionCount);
        Assert.Null(harness.Wrapper.FindSession(silentAddress));
        Assert.NotNull(harness.Wrapper.FindSession(chattyAddress));

        // The chatty peer's own game packet is in the log too, so pick the notification out by id.
        Received timeout = Assert.Single(
            harness.Received,
            entry => entry.Id == PacketId.PACKET_ID_PLAYER_TIMEOUT);
        Assert.Equal(silentAddress, timeout.Address);
    }
}
