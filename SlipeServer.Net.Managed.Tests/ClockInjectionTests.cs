using System;
using System.Linq;
using SlipeServer.Net.Managed;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Tests that the behavioural time reads in <see cref="MtaNetWrapper"/> and
/// <see cref="Gate1RateLimiter"/> come from the injected <see cref="IMonotonicClock"/>.
///
/// <para>
/// Every boundary here is reached by advancing a <see cref="ManualClock"/>. There is no
/// <c>Thread.Sleep</c> and no <c>Task.Delay</c>: the 10-second ordering and split timeouts and the
/// 60,001 ms Gate 1 sweep are asserted at the millisecond, and a test cannot be flaky because the
/// machine was busy.
/// </para>
///
/// <para>
/// The datagram literals are transcribed by hand from the datagram framing, not produced by
/// <see cref="RakNetDatagramWriter"/>. A round trip through the port's own encoder would only prove
/// that the port agrees with itself.
/// </para>
/// </summary>
public class ClockInjectionTests
{
    /// <summary><c>seq=20 rel=3 ch=0 oi=1</c>, payload <c>85 31</c>.</summary>
    private const string OrderedIndexOne =
        "05000000180040000010808531";

    /// <summary><c>seq=22 rel=3 ch=0 oi=2</c>, payload <c>85 32</c>.</summary>
    private const string OrderedIndexTwo =
        "05800000180080000010808532";

    /// <summary><c>seq=21 rel=3 ch=0 oi=0</c>, payload <c>85 30</c>.</summary>
    private const string OrderedIndexZero =
        "05400000180000000010808530";

    /// <summary>
    /// <c>seq=30 rel=3 ch=0 oi=0 split=0/2 sid=0</c>, 16 bits, payload <c>85 30</c>.
    /// </summary>
    private const string SplitPartZero =
        "07800000180000000020001E1E50808530";

    /// <summary>
    /// <c>seq=31 rel=3 ch=0 oi=0 split=1/2 sid=0</c>, 8 bits, payload <c>31</c>.
    /// </summary>
    private const string SplitPartOne =
        "07C00000180000000020001E3E5C0031";

    /// <summary><c>seq=40 rel=3 ch=0 oi=0</c>, wire id <c>0x76</c> — post-swap mtaId 4.</summary>
    private const string PlayerJoinDataZero =
        "0A000000180000000010807601";

    /// <summary><c>seq=41 rel=3 ch=0 oi=1</c>, wire id <c>0x76</c>.</summary>
    private const string PlayerJoinDataOne =
        "0A400000180040000010807602";

    /// <summary><c>seq=42 rel=3 ch=0 oi=2</c>, wire id <c>0x76</c>.</summary>
    private const string PlayerJoinDataTwo =
        "0A800000180080000010807603";

    /// <summary>
    /// <c>0x26</c> subtype 1: <c>[0x26][magic 16][0x01][echo 11 22 33 44]</c>.
    /// </summary>
    private const string MtaHandshake38SubtypeOne =
        "2600FF00FFFEFEFEFEFDFDFDFD785634170111223344";

    /// <summary>
    /// The hand-built split vectors really do carry the fields their names claim, checked against the
    /// port's reader only. If this fails the vectors are wrong, not the reassembler.
    /// </summary>
    [Fact]
    public void TheTranscribedSplitVectorsParseAsDocumented()
    {
        AssertSplitVector(SplitPartZero, 30, 0, 16, new byte[] { 0x85, 0x30 });
        AssertSplitVector(SplitPartOne, 31, 1, 8, new byte[] { 0x31 });
    }

    [Fact]
    public void AnOrderedHoldSurvivesJustUnderTheTimeout()
    {
        var clock = new ManualClock();
        using var harness = new WrapperHarness(clock: clock);
        harness.ConnectSession();

        harness.InjectRawHex(OrderedIndexOne);
        Assert.Empty(harness.Received);

        // 9,999 ms: still inside the hold window, so index 2 joins index 1 behind the hole.
        clock.Advance(InboundOrdering.TimeoutMilliseconds - 1);
        harness.InjectRawHex(OrderedIndexTwo);
        Assert.Empty(harness.Received);

        harness.InjectRawHex(OrderedIndexZero);
        Assert.Equal(new[] { '0', '1', '2' }, harness.Received.Select(received => (char)received.Data[0]));
    }

    [Fact]
    public void AnOrderedHoldIsEvictedAtExactlyTheTimeout()
    {
        var clock = new ManualClock();
        using var harness = new WrapperHarness(clock: clock);
        harness.ConnectSession();

        harness.InjectRawHex(OrderedIndexOne);
        Assert.Empty(harness.Received);

        // 10,000 ms: the hole is skipped, so the stream resumes at the oldest held index. Index 0
        // was never delivered and is not invented.
        clock.Advance(InboundOrdering.TimeoutMilliseconds);
        harness.InjectRawHex(OrderedIndexTwo);

        Assert.Equal(new[] { '1', '2' }, harness.Received.Select(received => (char)received.Data[0]));
    }

    [Fact]
    public void ASplitMessageStillCompletesJustUnderTheTimeout()
    {
        var clock = new ManualClock();
        using var harness = new WrapperHarness(clock: clock);
        harness.ConnectSession();

        harness.InjectRawHex(SplitPartZero);
        Assert.Empty(harness.Received);

        clock.Advance(SplitReassembler.TimeoutMilliseconds - 1);
        harness.InjectRawHex(SplitPartOne);

        Assert.Equal(new byte[] { 0x30, 0x31 }, Assert.Single(harness.Received).Data);
        Assert.Contains(harness.Trace, line => line.Contains("reassembled", StringComparison.Ordinal));
    }

    [Fact]
    public void ASplitMessageIsEvictedAtExactlyTheTimeout()
    {
        var clock = new ManualClock();
        using var harness = new WrapperHarness(clock: clock);
        harness.ConnectSession();

        harness.InjectRawHex(SplitPartZero);

        // 10,000 ms: the pending message is dropped before part 1 is offered, so part 1 starts a new
        // incomplete message instead of completing the old one.
        clock.Advance(SplitReassembler.TimeoutMilliseconds);
        harness.InjectRawHex(SplitPartOne);

        Assert.Empty(harness.Received);
        Assert.Contains(
            harness.Trace,
            line => line.Contains("buffered split part 1/2", StringComparison.Ordinal));
        Assert.DoesNotContain(
            harness.Trace,
            line => line.Contains("reassembled", StringComparison.Ordinal));
    }

    /// <summary>
    /// The sweep interval is 60,001 ms and the window here is deliberately longer, so only the sweep
    /// — not the per-address window reset — can let the next packet through.
    /// </summary>
    [Fact]
    public void TheRateWindowExpiresOnASweepDrivenByTheInjectedClock()
    {
        var clock = new ManualClock();
        var limiter = new Gate1RateLimiter(clock);
        var options = new MtaAdmissionOptions { Gate1RateLimit = 1, Gate1WindowMs = 120_000 };
        const uint ip = 0x0A0B0C0D;

        Assert.True(limiter.Allow(ip, 4, options));
        Assert.True(limiter.Allow(ip, 4, options));
        Assert.False(limiter.Allow(ip, 4, options));
        Assert.Equal(1, limiter.TrackedAddressCount);

        // 60,000 ms: the sweep is not due yet and the window is still open.
        clock.Advance(60_000);
        Assert.False(limiter.Allow(ip, 4, options));

        // 60,001 ms: the sweep drops the stale address, so the next packet opens a fresh window.
        clock.Advance(1);
        Assert.True(limiter.Allow(ip, 4, options));
        Assert.Equal(1, limiter.TrackedAddressCount);
    }

    /// <summary>
    /// The regression test for the old process-wide Gate 1 table: two wrappers with independent
    /// clocks must each own their rate window. With a shared table, <c>second</c>'s first packet
    /// would already be the third for this IP and be dropped.
    /// </summary>
    [Fact]
    public void TwoWrappersWithIndependentClocksDoNotShareAGate1Window()
    {
        var firstClock = new ManualClock();
        var secondClock = new ManualClock();
        using var first = new WrapperHarness(clock: firstClock);
        using var second = new WrapperHarness(clock: secondClock);

        foreach (WrapperHarness harness in new[] { first, second })
        {
            harness.ConnectSession();
            harness.Wrapper.AdmissionOptions.Gate1RateLimit = 1;
            harness.Wrapper.AdmissionOptions.Gate1WindowMs = 120_000;
        }

        first.InjectRawHex(PlayerJoinDataZero);
        first.InjectRawHex(PlayerJoinDataOne);
        second.InjectRawHex(PlayerJoinDataZero);
        second.InjectRawHex(PlayerJoinDataOne);

        Assert.Equal(2, first.Received.Count);
        Assert.Equal(2, second.Received.Count);

        first.InjectRawHex(PlayerJoinDataTwo);
        Assert.Equal(2, first.Received.Count);
        Assert.Contains(first.Trace, line => line.Contains("gate 1", StringComparison.Ordinal));

        // The second wrapper's clock has not moved, so its own window is still open: this is its
        // third packet and it is dropped by its own limiter, not by the first wrapper's count.
        second.InjectRawHex(PlayerJoinDataTwo);
        Assert.Equal(2, second.Received.Count);
        Assert.Contains(second.Trace, line => line.Contains("gate 1", StringComparison.Ordinal));
    }

    [Fact]
    public void TheOfflineReplyTimestampComesFromTheInjectedClock()
    {
        var clock = new ManualClock(0x0000_0001_2345_6789UL);
        using var harness = new WrapperHarness(clock: clock);

        harness.InjectRawHex(MtaHandshake38SubtypeOne);
        harness.InjectRawHex(MtaHandshake38SubtypeOne);

        Assert.Equal(2, harness.Tx.Count);
        Assert.Equal(26, harness.Tx[0].Length);

        // Same server identity and same clock value, so two runs are byte-identical.
        Assert.Equal(harness.Tx[0].Bytes, harness.Tx[1].Bytes);

        // [0x26][magic 16][0x02][echo 11 22 33 44][low 32 bits of the clock, little-endian].
        Assert.Equal(new byte[] { 0x02 }, harness.Tx[0].Bytes[17..18]);
        Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 0x44 }, harness.Tx[0].Bytes[18..22]);
        Assert.Equal(new byte[] { 0x89, 0x67, 0x45, 0x23 }, harness.Tx[0].Bytes[22..26]);
    }

    private static void AssertSplitVector(
        string hex, uint sequence, uint splitIndex, int bitCount, byte[] payload)
    {
        byte[] datagram = WireTestKit.Hex(hex);
        Assert.True(
            RakNetDatagram.TryParse(datagram, datagram.Length, out RakNetDatagram parsed, out string error),
            error);

        RakNetPacketRecord record = Assert.Single(parsed.Packets);
        Assert.Equal(sequence, record.SequenceNumber);
        Assert.Equal((byte)WireReliability.ReliableOrdered, record.Reliability);
        Assert.Equal(0, record.OrderingChannel);
        Assert.Equal(0u, record.OrderingIndex);
        Assert.True(record.HasSplitData);
        Assert.Equal(0, record.SplitPacketId);
        Assert.Equal(splitIndex, record.SplitPacketIndex);
        Assert.Equal(2u, record.SplitPacketCount);
        Assert.Equal(bitCount, record.BitCount);
        Assert.Equal(payload, record.Payload);
    }
}
