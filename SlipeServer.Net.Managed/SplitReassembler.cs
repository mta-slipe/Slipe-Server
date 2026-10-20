namespace SlipeServer.Net.Managed;

/// <summary>
/// Reassembles inbound split packets before the MTA layer sees them.
///
/// <para>
/// RakNet fragments one logical message into <c>splitPacketCount</c> parts that share a
/// <c>splitPacketId</c> and are numbered <c>splitPacketIndex</c> 0..count-1. Every part carries its
/// own bit length and its own reliability header, and its payload begins byte-aligned inside the
/// datagram, so the logical message is the bit-wise concatenation of the part payloads in index
/// order — not a byte concatenation of the datagram bodies. The MTA layer must receive the
/// reassembled message as a single packet; delivering parts individually hands it several packets
/// whose first byte is an arbitrary mid-message byte, which is neither a wire id nor a valid RPC
/// payload.
/// </para>
///
/// <para>
/// The bounds below are defensive, not wire facts: the native implementation's own limits are
/// unknown. They exist so a peer cannot make the server allocate without limit. The per-message
/// part table is keyed by index rather than sized from the declared count for the same reason.
/// </para>
/// </summary>
internal sealed class SplitReassembler
{
    /// <summary>Concurrent incomplete messages buffered for one peer.</summary>
    internal const int MaxPendingMessages = 32;

    /// <summary>An incomplete message older than this is dropped, so one lost part cannot pin memory.</summary>
    internal const ulong TimeoutMilliseconds = 10_000;

    /// <summary>Largest logical message accepted, in bits (4 MiB).</summary>
    internal const int MaxMessageBits = 4 * 1024 * 1024 * 8;

    private readonly Dictionary<ushort, Pending> pending = new();

    /// <summary>Incomplete messages currently buffered. Diagnostics and tests only.</summary>
    internal int PendingMessageCount => this.pending.Count;

    /// <summary>
    /// Offers one split part. Returns <see cref="SplitAcceptance.Complete"/> with the reassembled
    /// message once the last missing part arrives, <see cref="SplitAcceptance.Buffered"/> while the
    /// message is still incomplete, and <see cref="SplitAcceptance.Discarded"/> when the part cannot
    /// contribute to any message.
    /// </summary>
    internal SplitAcceptance Accept(
        RakNetPacketRecord part, ulong nowMilliseconds, out RakNetPacketRecord? message)
    {
        message = null;

        if (!part.HasSplitData || part.BitCount <= 0 || part.SplitPacketIndex >= part.SplitPacketCount)
            return SplitAcceptance.Discarded;

        this.EvictExpired(nowMilliseconds);

        ushort id = part.SplitPacketId;
        if (this.pending.TryGetValue(id, out Pending? existing) && existing.Count != part.SplitPacketCount)
        {
            // The same id now describes a differently shaped message, so the buffered parts can never
            // complete it. Start over rather than mixing two messages together.
            this.pending.Remove(id);
            existing = null;
        }

        // A repeat index is a part the peer re-sent under a new sequence number, so the reliability
        // layer's sequence-number dedupe did not catch it. First copy wins.
        if (existing is not null && existing.Parts.ContainsKey(part.SplitPacketIndex))
            return SplitAcceptance.Discarded;

        if (existing is null)
        {
            if (this.pending.Count >= MaxPendingMessages)
                this.DropOldest();

            existing = new Pending(part.SplitPacketCount, nowMilliseconds);
            this.pending.Add(id, existing);
        }

        existing.Parts[part.SplitPacketIndex] = part;
        existing.Received++;

        if (existing.Received < existing.Count)
            return SplitAcceptance.Buffered;

        this.pending.Remove(id);

        return TryCombine(existing, out message) ? SplitAcceptance.Complete : SplitAcceptance.Discarded;
    }

    private void EvictExpired(ulong nowMilliseconds)
    {
        if (this.pending.Count == 0)
            return;

        List<ushort>? expired = null;
        foreach (KeyValuePair<ushort, Pending> entry in this.pending)
        {
            if (nowMilliseconds - entry.Value.FirstSeenMilliseconds < TimeoutMilliseconds)
                continue;

            (expired ??= new List<ushort>()).Add(entry.Key);
        }

        if (expired is null)
            return;

        foreach (ushort id in expired)
            this.pending.Remove(id);
    }

    private void DropOldest()
    {
        ushort oldestId = 0;
        ulong oldest = ulong.MaxValue;

        foreach (KeyValuePair<ushort, Pending> entry in this.pending)
        {
            if (entry.Value.FirstSeenMilliseconds >= oldest)
                continue;

            oldest = entry.Value.FirstSeenMilliseconds;
            oldestId = entry.Key;
        }

        this.pending.Remove(oldestId);
    }

    private static bool TryCombine(Pending entry, out RakNetPacketRecord? message)
    {
        message = null;

        long totalBits = 0;
        for (uint i = 0; i < entry.Count; i++)
        {
            if (!entry.Parts.TryGetValue(i, out RakNetPacketRecord? part) ||
                part.BitCount <= 0 ||
                part.BitCount > part.Payload.Length * 8)
            {
                return false;
            }

            totalBits += part.BitCount;
            if (totalBits > MaxMessageBits)
                return false;
        }

        if (totalBits <= 0)
            return false;

        var payload = new byte[(int)((totalBits + 7) >> 3)];
        int offset = 0;
        for (uint i = 0; i < entry.Count; i++)
        {
            RakNetPacketRecord part = entry.Parts[i];
            CopyBits(part.Payload, part.BitCount, payload, offset);
            offset += part.BitCount;
        }

        RakNetPacketRecord first = entry.Parts[0];
        message = new RakNetPacketRecord
        {
            SequenceNumber = first.SequenceNumber,
            Reliability = first.Reliability,
            OrderingChannel = first.OrderingChannel,
            OrderingIndex = first.OrderingIndex,
            BitCount = (int)totalBits,
            Payload = payload,
        };

        return true;
    }

    /// <summary>
    /// Copies <paramref name="bitCount"/> bits MSB-first, the bit order <see cref="BitStream"/> uses.
    /// </summary>
    private static void CopyBits(byte[] source, int bitCount, byte[] destination, int destinationBitOffset)
    {
        for (int i = 0; i < bitCount; i++)
        {
            if ((source[i >> 3] & (0x80 >> (i & 7))) == 0)
                continue;

            int target = destinationBitOffset + i;
            destination[target >> 3] |= (byte)(0x80 >> (target & 7));
        }
    }

    private sealed class Pending
    {
        internal Pending(uint count, ulong firstSeenMilliseconds)
        {
            this.Count = count;
            this.FirstSeenMilliseconds = firstSeenMilliseconds;
        }

        /// <summary>The message's declared part count.</summary>
        internal uint Count { get; }

        /// <summary>
        /// Parts received so far, keyed by <c>splitPacketIndex</c>.
        ///
        /// <para>
        /// Keyed rather than an array indexed by the declared count, because that count is a wire
        /// field: <c>splitPacketCount</c> may be up to 0x4000 (the parser's sanity cap), so an array
        /// sized from it would turn one 20-byte datagram into a 128 KiB allocation, and
        /// <see cref="MaxPendingMessages"/> of those into 4 MiB per peer. Keyed storage is
        /// proportional to the parts the peer actually sent, which is the only thing it controls.
        /// </para>
        /// </summary>
        internal Dictionary<uint, RakNetPacketRecord> Parts { get; } = new();

        internal ulong FirstSeenMilliseconds { get; }

        internal uint Received { get; set; }
    }
}

/// <summary>The outcome of offering one split part to <see cref="SplitReassembler"/>.</summary>
internal enum SplitAcceptance
{
    /// <summary>The part was buffered; the logical message is still incomplete.</summary>
    Buffered,

    /// <summary>The part completed the message, which was returned reassembled.</summary>
    Complete,

    /// <summary>The part was dropped: a repeat, an out-of-range index, or over a defensive bound.</summary>
    Discarded,
}