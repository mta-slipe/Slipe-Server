using System;
using System.Collections.Generic;
using System.Linq;
using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Conformance tests for the inbound ordering state machine, the sibling of
/// <see cref="SplitReassemblyTests"/>..
///
/// <para>
/// <c>capture.log</c> cannot settle this: its 110 sequenced receive packets run <c>seq</c> 0–109 with no
/// gaps and no non-monotonic step, because a healthy client on a healthy LAN never reorders. So every
/// stream here is synthesised. The end-to-end vectors below are transcribed by hand from the datagram
/// framing and are <b>not</b> produced by
/// <see cref="RakNetDatagramWriter"/>: a round trip through the port's own encoder would only prove
/// that the port agrees with itself.
/// </para>
///
/// <para>
/// The native behaviour on a <c>RELIABLE_ORDERED</c> index that is ahead of the hole is a
/// <b>hold</b>, not a rejection: the <c>0x100504d0</c> mismatch arm increments the out-of-order counter
/// and calls <c>FUN_1004d600</c>, which inserts the packet into a per-channel tree and returns. It is
/// only over a memory bound that the packet is freed.
/// </para>
/// </summary>
public class InboundOrderingTests
{
    /// <summary>
    /// The contract: index 1 arrives first, is held, and is delivered <em>after</em> index 0, in
    /// ordering-index order rather than arrival order.
    /// </summary>
    [Fact]
    public void AnOrderedPacketAheadOfTheHoleIsHeldUntilTheHoleFills()
    {
        var ordering = new InboundOrdering();

        Assert.Equal(OrderingAcceptance.Buffered, ordering.Accept(Ordered(0, 1), 0, out RakNetPacketRecord[] held));
        Assert.Empty(held);

        Assert.Equal(OrderingAcceptance.Deliver, ordering.Accept(Ordered(0, 0), 0, out RakNetPacketRecord[] ready));
        Assert.Equal(new uint[] { 0, 1 }, ready.Select(record => record.OrderingIndex));
    }

    /// <summary>Filling the hole releases the whole contiguous run, not just the next index.</summary>
    [Fact]
    public void FillingTheHoleDrainsEveryContiguousHeldRecord()
    {
        var ordering = new InboundOrdering();

        Assert.Equal(OrderingAcceptance.Buffered, ordering.Accept(Ordered(0, 3), 0, out _));
        Assert.Equal(OrderingAcceptance.Buffered, ordering.Accept(Ordered(0, 2), 0, out _));
        Assert.Equal(OrderingAcceptance.Buffered, ordering.Accept(Ordered(0, 1), 0, out _));

        Assert.Equal(OrderingAcceptance.Deliver, ordering.Accept(Ordered(0, 0), 0, out RakNetPacketRecord[] ready));
        Assert.Equal(new uint[] { 0, 1, 2, 3 }, ready.Select(record => record.OrderingIndex));
    }

    /// <summary>A non-contiguous hold is not released by an unrelated index: 2 waits for 1.</summary>
    [Fact]
    public void AGapInsideTheHoldQueueIsNotBridged()
    {
        var ordering = new InboundOrdering();

        ordering.Accept(Ordered(0, 1), 0, out _);
        ordering.Accept(Ordered(0, 3), 0, out _);

        Assert.Equal(OrderingAcceptance.Deliver, ordering.Accept(Ordered(0, 0), 0, out RakNetPacketRecord[] ready));
        Assert.Equal(new uint[] { 0, 1 }, ready.Select(record => record.OrderingIndex));

        Assert.Equal(OrderingAcceptance.Deliver, ordering.Accept(Ordered(0, 2), 0, out RakNetPacketRecord[] drained));
        Assert.Equal(new uint[] { 2, 3 }, drained.Select(record => record.OrderingIndex));
    }

    /// <summary>An ordered index behind the expected one is dropped, never delivered late.</summary>
    [Fact]
    public void AnOrderedPacketBehindTheHoleIsDiscarded()
    {
        var ordering = new InboundOrdering();

        ordering.Accept(Ordered(0, 0), 0, out _);
        ordering.Accept(Ordered(0, 1), 0, out _);

        Assert.Equal(OrderingAcceptance.Discarded, ordering.Accept(Ordered(0, 0, sequence: 90), 0, out RakNetPacketRecord[] ready));
        Assert.Empty(ready);
    }

    /// <summary>
    /// A sequenced stream may lose packets, so an index ahead of the window is delivered at once and
    /// only an older index is dropped.
    /// </summary>
    [Fact]
    public void ASequencedPacketAheadIsDeliveredImmediatelyAndAnOlderOneIsDropped()
    {
        var ordering = new InboundOrdering();

        Assert.Equal(OrderingAcceptance.Deliver, ordering.Accept(Sequenced(0, 5), 0, out RakNetPacketRecord[] ready));
        Assert.Equal(new uint[] { 5 }, ready.Select(record => record.OrderingIndex));

        Assert.Equal(OrderingAcceptance.Discarded, ordering.Accept(Sequenced(0, 3, sequence: 91), 0, out _));
    }

    /// <summary>Channels are independent: a hole on channel 1 does not hold channel 0.</summary>
    [Fact]
    public void ChannelsKeepIndependentExpectedIndexes()
    {
        var ordering = new InboundOrdering();

        Assert.Equal(OrderingAcceptance.Buffered, ordering.Accept(Ordered(1, 7), 0, out _));
        Assert.Equal(OrderingAcceptance.Deliver, ordering.Accept(Ordered(0, 0), 0, out RakNetPacketRecord[] ready));
        Assert.Equal(new uint[] { 0 }, ready.Select(record => record.OrderingIndex));
    }

    /// <summary>
    /// The channel field is five bits wide, so a channel outside it is a framing error rather than a
    /// stream: the native layer asserts on <c>channel &gt;= 0x20</c>.
    /// </summary>
    [Fact]
    public void AChannelBeyondTheFiveBitFieldIsDiscarded()
    {
        var ordering = new InboundOrdering();

        Assert.Equal(OrderingAcceptance.Discarded, ordering.Accept(Ordered(32, 0), 0, out _));
        Assert.Equal(OrderingAcceptance.Discarded, ordering.Accept(Sequenced(200, 0), 0, out _));
    }

    /// <summary>
    /// Reliabilities 0 and 2 carry no ordering field, so they pass through whatever their index says —
    /// including a stale one that would have been dropped had it been ordered.
    /// </summary>
    [Theory]
    [InlineData(WireReliability.Unreliable)]
    [InlineData(WireReliability.Reliable)]
    public void ReliabilitiesWithoutAnOrderingFieldBypassOrdering(WireReliability reliability)
    {
        var ordering = new InboundOrdering();
        ordering.Accept(Ordered(0, 4), 0, out _);

        var record = new RakNetPacketRecord
        {
            Reliability = (byte)reliability,
            OrderingChannel = 0,
            OrderingIndex = 0,
            Payload = new byte[] { 0x85, 0x00 },
        };

        Assert.Equal(OrderingAcceptance.Deliver, ordering.Accept(record, 0, out RakNetPacketRecord[] ready));
        Assert.Same(record, Assert.Single(ready));
    }

    /// <summary>
    /// A re-sent ordered packet keeps its ordering index but takes a new sequence number, so the
    /// sequence-number dedupe upstream cannot catch it. The first copy wins.
    /// </summary>
    [Fact]
    public void AReSentOrderedPacketKeepsTheFirstCopy()
    {
        var ordering = new InboundOrdering();

        RakNetPacketRecord first = Ordered(0, 1, sequence: 10);
        RakNetPacketRecord second = Ordered(0, 1, sequence: 11);

        Assert.Equal(OrderingAcceptance.Buffered, ordering.Accept(first, 0, out _));
        Assert.Equal(OrderingAcceptance.Discarded, ordering.Accept(second, 0, out _));

        ordering.Accept(Ordered(0, 0), 0, out RakNetPacketRecord[] ready);
        Assert.Same(first, ready[1]);
    }

    /// <summary>The hold queue is bounded, so a peer cannot pin memory by never filling the hole.</summary>
    [Fact]
    public void TheHoldQueueIsBoundedPerChannel()
    {
        var ordering = new InboundOrdering();

        for (uint index = 1; index <= InboundOrdering.MaxPendingMessagesPerChannel; index++)
            Assert.Equal(OrderingAcceptance.Buffered, ordering.Accept(Ordered(0, index), 0, out _));

        Assert.Equal(
            OrderingAcceptance.Discarded,
            ordering.Accept(Ordered(0, InboundOrdering.MaxPendingMessagesPerChannel + 1), 0, out _));
    }

    /// <summary>
    /// The live defect, in the shape the two-client run produced: one ordering line, a hold at
    /// <c>ch=0 oi=5</c> behind a hole that nothing ever filled
    /// (<c>logs/poc-20261018-172647.log:39</c>), and then silence on that channel — which is "client
    /// A can no longer chat" once B joins, because chat is a reliable-ordered RPC on channel 0.
    ///
    /// <para>
    /// The old eviction dropped the held record and left the expectation where it was, so every later
    /// index stayed ahead of the hole and expired in turn: the channel was pinned for the rest of the
    /// session. A hole that outlives the timeout must instead be skipped, resuming the stream at the
    /// oldest index still waiting behind it.
    /// </para>
    /// </summary>
    [Fact]
    public void AHoleThatOutlivesTheTimeoutIsSkippedAndTheChannelResumes()
    {
        var ordering = new InboundOrdering();

        // Indexes 0-4 are lost. Index 5 is the one line the live run logged: held behind the hole.
        Assert.Equal(OrderingAcceptance.Buffered, ordering.Accept(Ordered(0, 5), 0, out RakNetPacketRecord[] held));
        Assert.Empty(held);

        // Index 6 arrives with the hole still open and still young: also held.
        Assert.Equal(
            OrderingAcceptance.Buffered,
            ordering.Accept(Ordered(0, 6), InboundOrdering.TimeoutMilliseconds - 1, out _));

        // Once the hole outlives the timeout the next arrival releases the held run instead of being
        // held behind it forever.
        Assert.Equal(
            OrderingAcceptance.Deliver,
            ordering.Accept(Ordered(0, 7), InboundOrdering.TimeoutMilliseconds, out RakNetPacketRecord[] ready));
        Assert.Equal(new uint[] { 5, 6, 7 }, ready.Select(record => record.OrderingIndex));

        // And the channel is usable again: the next index delivers on its own.
        Assert.Equal(
            OrderingAcceptance.Deliver,
            ordering.Accept(Ordered(0, 8), InboundOrdering.TimeoutMilliseconds, out RakNetPacketRecord[] after));
        Assert.Equal(new uint[] { 8 }, after.Select(record => record.OrderingIndex));
    }

    /// <summary>
    /// A hole that is still young when the missing packet arrives is filled, not skipped: the late
    /// packet delivers and releases everything behind it. A timeout must not turn a slow packet into
    /// a lost one.
    /// </summary>
    [Fact]
    public void AHoleFilledAtTheTimeoutStillDeliversInOrder()
    {
        var ordering = new InboundOrdering();

        ordering.Accept(Ordered(0, 1), 0, out _);
        ordering.Accept(Ordered(0, 2), 0, out _);

        Assert.Equal(
            OrderingAcceptance.Deliver,
            ordering.Accept(Ordered(0, 0), InboundOrdering.TimeoutMilliseconds, out RakNetPacketRecord[] ready));
        Assert.Equal(new uint[] { 0, 1, 2 }, ready.Select(record => record.OrderingIndex));
    }

    /// <summary>
    /// <see cref="InboundOrdering.Prime"/> only moves an expectation forward, so a test cannot use it
    /// to rewind state and make the layer deliver a packet it should drop.
    /// </summary>
    [Fact]
    public void PrimingOnlyMovesAnExpectationForward()
    {
        var ordering = new InboundOrdering();

        ordering.Accept(Ordered(0, 0), 0, out _);
        ordering.Accept(Ordered(0, 1), 0, out _);

        ordering.Prime(0, 0, 0);

        Assert.Equal(OrderingAcceptance.Discarded, ordering.Accept(Ordered(0, 0, sequence: 92), 0, out _));
    }

    private static RakNetPacketRecord Ordered(byte channel, uint index, uint sequence = 0) =>
        Record(WireReliability.ReliableOrdered, channel, index, sequence);

    private static RakNetPacketRecord Sequenced(byte channel, uint index, uint sequence = 0) =>
        Record(WireReliability.ReliableSequenced, channel, index, sequence);

    private static RakNetPacketRecord Record(WireReliability reliability, byte channel, uint index, uint sequence) =>
        new()
        {
            SequenceNumber = sequence,
            Reliability = (byte)reliability,
            OrderingChannel = channel,
            OrderingIndex = index,
            Payload = new byte[] { 0x85, (byte)('0' + (index % 10)) },
        };
}

/// <summary>
/// The same contract, end to end through <see cref="MtaNetWrapper"/>'s receive path, using datagrams
/// transcribed by hand from the datagram framing rather than written by the port's encoder.
///
/// <para>
/// Every vector below is 13 bytes: two header bits (<c>hasAcks=0</c>, <c>hasSecond=0</c>), the 32-bit
/// sequence number, the 3-bit reliability, the 5-bit channel and 32-bit ordering index, <c>hasSplit=0</c>,
/// the compressed 16-bit bit count, then the two payload bytes <c>85</c> (wire <c>0x85</c>, mtaId
/// <c>0x22</c>) and one ASCII marker. The bit count 16 compresses to <c>1 0 00010000</c> — high byte is
/// the zero fill byte, low byte is not the fill nibble — which is why the last three header bits are
/// <c>10</c> then a pad bit.
/// </para>
///
/// <para>
/// These use <see cref="WrapperHarness.InjectRaw"/>: the default <see cref="WrapperHarness.Inject"/>
/// declares a per-channel starting index for recorded slices, which would mask exactly the hole these
/// tests create.
/// </para>
/// </summary>
public class InboundOrderingWireTests
{
    /// <summary>
    /// <c>seq=20 rel=3 ch=0 oi=1</c>, payload <c>85 31</c>.
    /// </summary>
    private const string OrderedIndexOne =
        "05000000180040000010808531";

    /// <summary>
    /// <c>seq=21 rel=3 ch=0 oi=0</c>, payload <c>85 30</c>.
    /// </summary>
    private const string OrderedIndexZero =
        "05400000180000000010808530";

    /// <summary>
    /// <c>seq=22 rel=3 ch=0 oi=0</c>, payload <c>85 5A</c> — index 0 again after it was already
    /// delivered, with a fresh sequence number so the dedupe cannot catch it.
    /// </summary>
    private const string OrderedIndexZeroRepeat =
        "0580000018000000001080855A";

    /// <summary>
    /// <c>seq=30 rel=4 ch=0 oi=5</c>, payload <c>85 35</c>.
    /// </summary>
    private const string SequencedIndexFive =
        "07800000200140000010808535";

    /// <summary>
    /// <c>seq=31 rel=4 ch=0 oi=2</c>, payload <c>85 58</c> — behind the window opened by
    /// <see cref="SequencedIndexFive"/>.
    /// </summary>
    private const string SequencedIndexTwo =
        "07C00000200080000010808558";

    /// <summary>
    /// The transcribed literals really do carry the fields their names claim, checked against the
    /// port's <em>reader</em> only. If this fails the vectors are wrong, not the ordering layer.
    /// </summary>
    [Fact]
    public void TheTranscribedVectorsParseAsDocumented()
    {
        AssertVector(OrderedIndexOne, 20, WireReliability.ReliableOrdered, 0, 1, 0x31);
        AssertVector(OrderedIndexZero, 21, WireReliability.ReliableOrdered, 0, 0, 0x30);
        AssertVector(OrderedIndexZeroRepeat, 22, WireReliability.ReliableOrdered, 0, 0, 0x5A);
        AssertVector(SequencedIndexFive, 30, WireReliability.ReliableSequenced, 0, 5, 0x35);
        AssertVector(SequencedIndexTwo, 31, WireReliability.ReliableSequenced, 0, 2, 0x58);
    }

    [Fact]
    public void AReorderedOrderedStreamIsDeliveredInOrderingIndexOrder()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.InjectRawHex(OrderedIndexOne);
        Assert.Empty(harness.Received);

        harness.InjectRawHex(OrderedIndexZero);

        Assert.Equal(new[] { '0', '1' }, harness.Received.Select(received => (char)received.Data[0]));
    }

    /// <summary>
    /// Acking is on receipt, not on delivery: the held packet is acknowledged while it is still held,
    /// because the native layer acks the datagram inside the reliability layer, upstream of ordering.
    /// </summary>
    [Fact]
    public void AHeldOrderedPacketIsAcknowledgedBeforeItIsDelivered()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        int txBefore = harness.Tx.Count;
        harness.InjectRawHex(OrderedIndexOne);

        Assert.Empty(harness.Received);
        Assert.Contains(
            harness.Tx.Skip(txBefore),
            line => line.Classification.Contains("acks=1 ", StringComparison.Ordinal));
    }

    [Fact]
    public void AnOrderedIndexBehindTheHoleIsDropped()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.InjectRawHex(OrderedIndexZero);
        harness.InjectRawHex(OrderedIndexOne);
        harness.InjectRawHex(OrderedIndexZeroRepeat);

        Assert.Equal(new[] { '0', '1' }, harness.Received.Select(received => (char)received.Data[0]));
        Assert.Contains(harness.Trace, line => line.Contains("discarded", StringComparison.Ordinal));
    }

    [Fact]
    public void ASequencedSkipIsDeliveredAndAnOlderIndexIsDropped()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.InjectRawHex(SequencedIndexFive);
        harness.InjectRawHex(SequencedIndexTwo);

        Assert.Equal(new[] { '5' }, harness.Received.Select(received => (char)received.Data[0]));
    }

    private static void AssertVector(
        string hex, uint sequence, WireReliability reliability, byte channel, uint orderingIndex, byte marker)
    {
        byte[] datagram = WireTestKit.Hex(hex);
        Assert.True(RakNetDatagram.TryParse(datagram, datagram.Length, out RakNetDatagram parsed, out string error), error);

        RakNetPacketRecord record = Assert.Single(parsed.Packets);
        Assert.Equal(sequence, record.SequenceNumber);
        Assert.Equal((byte)reliability, record.Reliability);
        Assert.Equal(channel, record.OrderingChannel);
        Assert.Equal(orderingIndex, record.OrderingIndex);
        Assert.False(record.HasSplitData);
        Assert.Equal(16, record.BitCount);
        Assert.Equal(new byte[] { 0x85, marker }, record.Payload);
    }
}
