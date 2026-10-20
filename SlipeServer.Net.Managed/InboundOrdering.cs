namespace SlipeServer.Net.Managed;

/// <summary>
/// The inbound half of RakNet's ordering state machine, the sibling of <see cref="SplitReassembler"/>.
///
/// <para>
/// RakNet's reliability layer delivers in ordering-index order, not arrival order. Reliability 3
/// (<c>RELIABLE_ORDERED</c>) has a per-channel expected index: a packet that matches is delivered and
/// the next index becomes expected, a packet that is ahead waits in a per-channel pending queue until
/// the hole fills, and a packet that is behind is dropped. A hole that never fills is skipped after
/// <see cref="TimeoutMilliseconds"/>, which is what keeps one lost packet from stalling the channel.
/// Reliabilities 1 and 4 are sequenced rather than ordered — they carry an index too, but a sequenced
/// stream is allowed to lose packets, so a packet that is ahead is delivered immediately and only an
/// older one is dropped.
/// </para>
///
/// <para>
/// This runs after split reassembly and before admission, because ordering belongs to the
/// reliability layer, upstream of the MTA layer. It never touches the ack path: the wrapper acks
/// each datagram on receipt, so a packet held here is already acknowledged.
/// </para>
///
/// <para>
/// The bounds are deliberately much tighter than the native pending-ordered queue's (tens of
/// thousands of held messages and tens of MiB per channel). Those are server-side ceilings, not
/// wire facts, so this port keeps a small one of its own choosing.
/// </para>
/// </summary>
internal sealed class InboundOrdering
{
    /// <summary>Ordering channels the wire can carry: the channel field is 5 bits.</summary>
    internal const int MaxOrderingChannels = 32;

    /// <summary>Records held ahead of the hole, per channel.</summary>
    internal const int MaxPendingMessagesPerChannel = 32;

    /// <summary>
    /// How long a hole may stay open before the stream skips it. A held record older than this is
    /// not merely dropped: the channel resumes at the oldest record still waiting behind the hole.
    /// Dropping it without moving the expectation would stall the channel for the rest of the
    /// session — every later index stays ahead of the hole, is held, expires, and nothing ever
    /// moves the expectation past it.
    /// </summary>
    internal const ulong TimeoutMilliseconds = 10_000;

    private readonly ChannelState[] channels = CreateChannels();

    /// <summary>Records currently held ahead of a hole, across every channel. Diagnostics and tests only.</summary>
    internal int HeldRecordCount
    {
        get
        {
            int total = 0;
            foreach (ChannelState state in this.channels)
                total += state.HeldCount;
            return total;
        }
    }

    /// <summary>
    /// The ordered index a channel will deliver next. Diagnostics and tests only: it lets a caller
    /// distinguish "this record was dropped as stale because the channel already passed its index"
    /// from "this record was lost".
    /// </summary>
    internal uint ExpectedOrderedIndexFor(byte channel) =>
        channel < this.channels.Length ? this.channels[channel].ExpectedOrderedIndex : 0;

    /// <summary>
    /// Offers one record — already reassembled if it was a split part — to the ordering layer.
    /// </summary>
    /// <param name="record">The record to place.</param>
    /// <param name="nowMilliseconds">A monotonic millisecond clock for the hold timeout.</param>
    /// <param name="ready">
    /// The records to deliver, in order. Empty when nothing may be delivered yet; for an ordered
    /// record that filled the hole this is that record followed by every held record the hole
    /// unblocked, so a single call can release more than one packet.
    /// </param>
    internal OrderingAcceptance Accept(
        RakNetPacketRecord record, ulong nowMilliseconds, out RakNetPacketRecord[] ready)
    {
        ready = [];

        var reliability = (WireReliability)record.Reliability;
        if (!reliability.CarriesOrdering())
        {
            // Reliabilities 0 and 2 carry no ordering field at all, so arrival order is the only
            // order they have.
            ready = [record];
            return OrderingAcceptance.Deliver;
        }

        if (record.OrderingChannel >= MaxOrderingChannels)
            return OrderingAcceptance.Discarded;

        ChannelState state = this.channels[record.OrderingChannel];

        bool ordered = reliability == WireReliability.ReliableOrdered;
        uint expected = ordered ? state.ExpectedOrderedIndex : state.ExpectedSequencedIndex;

        // Half-space comparison: a difference whose top bit is set means "behind", which is the
        // unsigned, wrap-safe form of "older than expected".
        int distance = unchecked((int)(record.OrderingIndex - expected));

        // A record that fills the hole needs no eviction: it advances the expectation itself, and
        // draining then releases everything that was waiting behind it. Only a record that does not
        // fill the hole can leave it open long enough to matter.
        List<RakNetPacketRecord>? skipped = distance == 0 ? null : state.EvictExpired(nowMilliseconds);
        if (skipped is not null)
        {
            expected = ordered ? state.ExpectedOrderedIndex : state.ExpectedSequencedIndex;
            distance = unchecked((int)(record.OrderingIndex - expected));
        }

        if (distance < 0)
        {
            if (skipped is null)
                return OrderingAcceptance.Discarded;

            ready = skipped.ToArray();
            return OrderingAcceptance.Deliver;
        }

        if (!ordered)
        {
            // A sequenced stream may skip: the newest index wins and an older one is discarded above.
            state.ExpectedSequencedIndex = record.OrderingIndex + 1;
            if (skipped is null)
            {
                ready = [record];
            }
            else
            {
                skipped.Add(record);
                ready = skipped.ToArray();
            }

            return OrderingAcceptance.Deliver;
        }

        if (distance == 0)
        {
            state.ExpectedOrderedIndex = record.OrderingIndex + 1;
            List<RakNetPacketRecord> delivered = skipped ?? new List<RakNetPacketRecord>(1);
            delivered.Add(record);
            state.Drain(delivered);
            ready = delivered.ToArray();
            return OrderingAcceptance.Deliver;
        }

        bool held = state.Hold(record, nowMilliseconds);
        if (skipped is null)
            return held ? OrderingAcceptance.Buffered : OrderingAcceptance.Discarded;

        ready = skipped.ToArray();
        return OrderingAcceptance.Deliver;
    }

    /// <summary>
    /// Declares where a channel's stream resumes, for tests that inject a packet taken from the
    /// middle of a live session.
    ///
    /// <para>
    /// A recorded packet is a slice, not a stream: one with ordering index 4 on channel 0 would be
    /// held behind a hole that a fresh session, expecting 0, never fills. Raising the expectation
    /// stands in for the packets the recording had already delivered. It only ever moves an index
    /// forward, so a test cannot use it to rewind state or to make a packet the layer should drop
    /// deliver.
    /// </para>
    /// </summary>
    internal void Prime(byte channel, uint orderedIndex, uint sequencedIndex)
    {
        if (channel >= MaxOrderingChannels)
            return;

        ChannelState state = this.channels[channel];
        if (IsAhead(orderedIndex, state.ExpectedOrderedIndex))
            state.ExpectedOrderedIndex = orderedIndex;
        if (IsAhead(sequencedIndex, state.ExpectedSequencedIndex))
            state.ExpectedSequencedIndex = sequencedIndex;
    }

    private static ChannelState[] CreateChannels()
    {
        var states = new ChannelState[MaxOrderingChannels];
        for (int i = 0; i < states.Length; i++)
            states[i] = new ChannelState();
        return states;
    }

    /// <summary>Wrap-safe "is <paramref name="index"/> newer than <paramref name="expected"/>".</summary>
    private static bool IsAhead(uint index, uint expected) => unchecked((int)(index - expected)) > 0;

    private sealed class ChannelState
    {
        private readonly Dictionary<uint, HeldRecord> held = new();

        internal uint ExpectedOrderedIndex { get; set; }

        internal uint ExpectedSequencedIndex { get; set; }

        /// <summary>Records held behind the hole. Diagnostics and tests only.</summary>
        internal int HeldCount => this.held.Count;

        /// <summary>Releases every held record that the newly advanced index unblocks.</summary>
        internal void Drain(List<RakNetPacketRecord> delivered)
        {
            while (this.held.Remove(this.ExpectedOrderedIndex, out HeldRecord? next))
            {
                delivered.Add(next.Record);
                this.ExpectedOrderedIndex++;
            }
        }

        /// <summary>Holds one record ahead of the hole. False when it cannot be kept.</summary>
        internal bool Hold(RakNetPacketRecord record, ulong nowMilliseconds)
        {
            if (this.held.Count >= MaxPendingMessagesPerChannel)
                return false;

            // A re-sent ordered packet keeps its ordering index but takes a new sequence number, so
            // the sequence-number dedupe upstream cannot catch it. First copy wins.
            return this.held.TryAdd(record.OrderingIndex, new HeldRecord(record, nowMilliseconds));
        }

        /// <summary>
        /// Skips a hole that has outlived <see cref="TimeoutMilliseconds"/>, resuming the ordered
        /// stream at the oldest index still waiting behind it. Returns the records the skip released,
        /// or <c>null</c> when the hole is still young enough to wait for.
        ///
        /// <para>
        /// The hole is as old as the oldest record held behind it: a held record is by definition
        /// waiting for an index below its own, so the oldest one has been waiting on that gap the
        /// longest. RakNet's own pending-ordered queue also discards on this event, so skipping is
        /// standard behaviour; moving the expectation is this port's addition, without which the
        /// discards would leave the channel stuck forever.
        /// </para>
        /// </summary>
        internal List<RakNetPacketRecord>? EvictExpired(ulong nowMilliseconds)
        {
            if (this.held.Count == 0)
                return null;

            ulong oldest = ulong.MaxValue;
            foreach (HeldRecord entry in this.held.Values)
                oldest = Math.Min(oldest, entry.HeldSinceMilliseconds);

            if (nowMilliseconds - oldest < TimeoutMilliseconds)
                return null;

            this.ExpectedOrderedIndex = this.NearestHeldIndex();

            var released = new List<RakNetPacketRecord>();
            this.Drain(released);
            return released.Count > 0 ? released : null;
        }

        /// <summary>The smallest held index at or ahead of the expectation, wrap-safely.</summary>
        private uint NearestHeldIndex()
        {
            uint nearest = this.ExpectedOrderedIndex;
            int best = int.MaxValue;

            foreach (uint index in this.held.Keys)
            {
                int distance = unchecked((int)(index - this.ExpectedOrderedIndex));
                if (distance < best)
                {
                    best = distance;
                    nearest = index;
                }
            }

            return nearest;
        }
    }

    private sealed class HeldRecord
    {
        internal HeldRecord(RakNetPacketRecord record, ulong heldSinceMilliseconds)
        {
            this.Record = record;
            this.HeldSinceMilliseconds = heldSinceMilliseconds;
        }

        internal RakNetPacketRecord Record { get; }

        internal ulong HeldSinceMilliseconds { get; }
    }
}

/// <summary>The outcome of offering one record to <see cref="InboundOrdering"/>.</summary>
internal enum OrderingAcceptance
{
    /// <summary>At least one record is ready; see the <c>ready</c> output.</summary>
    Deliver,

    /// <summary>The record is ahead of the hole and was held.</summary>
    Buffered,

    /// <summary>The record is older than expected, a repeat, or over a defensive bound.</summary>
    Discarded,
}
