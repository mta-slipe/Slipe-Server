using System.Net;

namespace SlipeServer.Net.Managed;

/// <summary>
/// Per-peer session record: handshake and admission latches, liveness, and reliability-layer
/// bookkeeping. Only the fields this port actually reads are modelled.
/// </summary>
public sealed class MtaNetSession
{
    /// <summary>State value for a fully established connection.</summary>
    public const int StateEstablished = 3;

    public MtaNetSession(ulong binaryAddress, IPEndPoint endPoint)
    {
        BinaryAddress = binaryAddress;
        EndPoint = endPoint;
        InboundPacketCounts = new ushort[256];
        InboundBitTotals = new int[256];
    }

    public ulong BinaryAddress { get; }

    public IPEndPoint EndPoint { get; }

    /// <summary>The client's 16-byte GUID, captured during the open-connection exchange.</summary>
    public byte[] Guid { get; set; } = new byte[16];

    /// <summary>
    /// The negotiated bitstream version, set when the join data is processed and cleared on quit.
    /// 0 means "not yet negotiated", which is the only state in which an inbound mtaId 4 is admitted.
    /// </summary>
    public ushort NegotiatedBitStreamVersion { get; set; }

    /// <summary>
    /// The id dialect this peer speaks, latched by <see cref="MtaIdDialect.Auto"/> from the wire id of
    /// its first join-data packet. Null means "not yet determined".
    /// </summary>
    public MtaIdDialect? DetectedDialect { get; set; }

    /// <summary>Post-handshake latch that makes gate 3 admit everything.</summary>
    public bool HandshakeComplete { get; set; }

    /// <summary>
    /// True once this port has emitted packet 2 (<c>SERVER_JOIN_COMPLETE</c>) to the peer. It is a
    /// latch kept at the transport layer so that emission stays idempotent.
    /// </summary>
    public bool JoinCompleteSent { get; set; }

    /// <summary>A live, non-rejected record.</summary>
    public bool IsEstablished { get; set; }

    /// <summary>
    /// Set once the connected <c>RID_CONNECTION_REQUEST</c> handler has accepted the password.
    /// Purely diagnostic in this port: it records that the 0x04/0x0E exchange happened, and does not
    /// gate anything.
    /// </summary>
    public bool ConnectionAccepted { get; set; }

    /// <summary>Peer rejected; a set flag drops everything.</summary>
    public bool Rejected { get; set; }

    /// <summary>Purpose unknown; a set flag drops everything.</summary>
    public bool Flag854 { get; set; }

    /// <summary>Purpose unknown; a set flag drops everything.</summary>
    public bool Flag855 { get; set; }

    /// <summary>A small counter compared against 2 by gates 3 and 4.</summary>
    public byte Counter97d { get; set; }

    /// <summary>Reserved handshake flag; purpose unknown.</summary>
    public bool Flag97e { get; set; }

    /// <summary>Reserved handshake flag; purpose unknown.</summary>
    public bool Flag97f { get; set; }

    // ---- gate 2 accumulators (per mtaId) ----

    public ushort[] InboundPacketCounts { get; }
    public int[] InboundBitTotals { get; }

    /// <summary>Flood-notice latch; once set it rejects everything from this peer.</summary>
    public bool FloodNoticeLatched { get; set; }

    /// <summary>Latch for the mtaId 0x2a notice.</summary>
    public bool CommandNoticeLatched { get; set; }

    // ---- client identity strings ----

    public string Serial { get; set; } = string.Empty;
    public string Extra { get; set; } = string.Empty;
    public string ClientVersion { get; set; } = string.Empty;

    // ---- liveness ----

    /// <summary>
    /// Monotonic time of the last datagram accepted from this peer, used to measure idle time before
    /// declaring the connection too old.
    ///
    /// <para>
    /// Deliberately <em>not</em> behind <see cref="ReliabilitySyncRoot"/>. It is one independent
    /// 64-bit word that participates in no invariant with the ack ledger or the sequence counters, so
    /// it needs no ordering against them — only that a read cannot tear. <see cref="Interlocked"/>
    /// gives exactly that, where taking the ledger lock on every inbound datagram would put the
    /// receive hot path behind a lock the retransmission timer also holds.
    /// </para>
    /// </summary>
    internal ulong LastReceivedAtMilliseconds
    {
        get => (ulong)Interlocked.Read(ref this.lastReceivedAtMilliseconds);
        private set => Interlocked.Exchange(ref this.lastReceivedAtMilliseconds, (long)value);
    }

    private long lastReceivedAtMilliseconds;

    /// <summary>
    /// Records that a datagram from this peer arrived at <paramref name="nowMilliseconds"/>. Called
    /// before any admission gate: liveness is "a datagram arrived", not "a packet was delivered", so
    /// a peer whose packets are all being dropped is still alive.
    /// </summary>
    internal void MarkReceived(ulong nowMilliseconds) => this.LastReceivedAtMilliseconds = nowMilliseconds;

    /// <summary>
    /// Claims the right to retire this record. The silence sweep and an inbound teardown can race on
    /// the same peer, and the claim is one-shot so exactly one of them delivers a terminal MTA packet.
    /// </summary>
    internal bool TryClaimRetirement() => Interlocked.Exchange(ref this.retirementClaimed, 1) == 0;

    private int retirementClaimed;

    // ---- reliability-layer bookkeeping ----

    /// <summary>
    /// Guards the reliability state that more than one thread touches: <see cref="PendingAcks"/>,
    /// <see cref="UnackedReliable"/> and the outbound sequence/ordering counters.
    ///
    /// <para>
    /// <see cref="PendingAcks"/> is appended from the receive loop and read-then-cleared from the send
    /// path, and the ledger and the retransmission timer are two more writers. Without a lock an ack
    /// could be silently dropped (or a torn <see cref="AckRange"/> read) whenever these paths
    /// interleave -- a defect that two clients never expose and sixty might -- so the whole block
    /// takes this lock.
    /// </para>
    ///
    /// <para>
    /// Inbound state — <see cref="InboundSplits"/>, <see cref="InboundOrdering"/> and
    /// <see cref="ReceivedSequences"/> — is deliberately outside it: only the single receive loop
    /// touches those, and a second lock there would buy nothing.
    /// </para>
    /// </summary>
    internal object ReliabilitySyncRoot { get; } = new();

    /// <summary>Sequence numbers of packets received but not yet acknowledged.</summary>
    public List<AckRange> PendingAcks { get; } = new();

    /// <summary>
    /// Sequence numbers already delivered to the message layer. A retransmission carries the same
    /// sequence number, so the reliability layer acks it again but delivers it only once; without
    /// this a peer stuck in retransmission would be answered once per retry.
    ///
    /// <para>
    /// Bounded by <see cref="MaxTrackedReceivedSequences"/> through
    /// <see cref="TrackReceivedSequence"/>. The reliability layer dedupes inside a fixed sliding
    /// window, not against an ever-growing history, and an unbounded set here would let any peer
    /// that completed the open-connection handshake grow the server's heap one 20-byte datagram at
    /// a time for as long as it kept inventing sequence numbers.
    /// </para>
    /// </summary>
    public HashSet<uint> ReceivedSequences { get; } = new();

    /// <summary>
    /// How many received sequence numbers are remembered per peer. Chosen, not derived: it has to
    /// cover the retransmissions a peer can still have in flight, which is bounded by the send rate
    /// over a few round trips, and 4096 is far above that at any plausible packet rate.
    /// </summary>
    internal const int MaxTrackedReceivedSequences = 4096;

    /// <summary>
    /// Records a received sequence number, returning <c>false</c> when it is a retransmission that
    /// was already delivered. Bounds <see cref="ReceivedSequences"/> so a peer cannot grow it
    /// without limit: once the set is full, everything more than half a window behind the newest
    /// sequence number is dropped. Those entries can never be asked about again — a retransmission
    /// that old is long past any acknowledgement the peer is still waiting for — so the window only
    /// has to be wide enough to cover what is genuinely still in flight.
    /// </summary>
    internal bool TrackReceivedSequence(uint sequence)
    {
        if (!this.ReceivedSequences.Add(sequence))
            return false;

        if (this.ReceivedSequences.Count > MaxTrackedReceivedSequences)
        {
            const uint Window = MaxTrackedReceivedSequences / 2;
            uint floor = sequence > Window ? sequence - Window : 0u;
            this.ReceivedSequences.RemoveWhere(tracked => tracked < floor);
        }

        return true;
    }

    /// <summary>Next outbound RakNet sequence number for this peer.</summary>
    public uint NextOutboundSequenceNumber { get; set; }

    /// <summary>
    /// Next outbound ordering index per ordering channel for the ordered family (reliability 3).
    /// The ordered and sequenced families keep independent index spaces per channel.
    /// </summary>
    public Dictionary<byte, uint> NextOrderedIndex { get; } = new();

    /// <summary>
    /// Next outbound ordering index per ordering channel for the sequenced family (reliabilities 1
    /// and 4), the sibling of <see cref="NextOrderedIndex"/>.
    /// </summary>
    public Dictionary<byte, uint> NextSequencedIndex { get; } = new();

    /// <summary>
    /// Next outbound split-packet id. Every part of one logical message shares the id; the peer
    /// reassembles on it, so it must not repeat while a message is still in flight.
    /// </summary>
    public ushort NextSplitPacketId { get; set; }

    /// <summary>
    /// Reliable outbound packets that have not been acknowledged yet, keyed by RakNet sequence
    /// number. Only <see cref="WireReliabilityExtensions.IsReliable"/> packets are retained: an
    /// unreliable packet has no retransmission contract, so it is never entered here.
    ///
    /// <para>
    /// Each entry stores the built <see cref="OutgoingPacket"/> rather than the payload, because a
    /// retransmission must reuse the original sequence number <em>and</em> ordering index. The peer
    /// dedupes on sequence number (<see cref="ReceivedSequences"/>) and orders on ordering index, so
    /// allocating a fresh one would turn the resend into a new packet and corrupt both.
    /// </para>
    ///
    /// <para>
    /// A split message is retained per part: each part has its own sequence number and is acked
    /// independently, so dropping one part resends one part.
    /// </para>
    /// </summary>
    internal Dictionary<uint, PendingReliablePacket> UnackedReliable { get; } = new();

    /// <summary>
    /// How many reliable packets one session may have in flight at once.
    ///
    /// <para>
    /// Without a count bound this ledger is bounded only by time: <c>TickRetransmissions</c> drops an
    /// entry after <see cref="RetransmissionPolicy.MaximumRetransmissions"/> attempts, so a peer that
    /// keeps eliciting reliable replies and never acks any of them grows the ledger at its own send
    /// rate. The host pays for that in per-peer memory, which is exactly what a remote attacker wants
    /// to spend someone else's memory on. The cap is a chosen constant, not a derived one (this
    /// port's own choice; not a claim about the native implementation), and it is deliberately
    /// generous: at the default retransmission timeout a healthy peer has a few packets in flight,
    /// and a peer that reaches 1024 has already stopped acking.
    /// </para>
    /// </summary>
    internal const int MaxUnackedReliablePackets = 1024;

    /// <summary>How many outbound reliable packets are still awaiting an ack. Diagnostics only.</summary>
    internal int UnackedReliableCount
    {
        get
        {
            lock (this.ReliabilitySyncRoot)
                return this.UnackedReliable.Count;
        }
    }

    /// <summary>
    /// Releases everything retained for this peer. Called from
    /// <c>MtaNetWrapper.RemoveSession</c> when the record is retired and from <c>Stop</c>: anything
    /// still held for a peer that is gone is a leak.
    /// </summary>
    internal void ReleaseReliabilityState()
    {
        lock (this.ReliabilitySyncRoot)
        {
            this.UnackedReliable.Clear();
            this.PendingAcks.Clear();
        }
    }

    /// <summary>
    /// Inbound split reassembly, keyed by the peer's split-packet id. Scoped to one connection, so a
    /// new session record starts with an empty buffer.
    /// </summary>
    internal SplitReassembler InboundSplits { get; } = new();

    /// <summary>
    /// Inbound ordering, the sibling of <see cref="InboundSplits"/>. Scoped to one connection so the
    /// expected index starts at 0 for every peer.
    /// </summary>
    internal InboundOrdering InboundOrdering { get; } = new();
}

/// <summary>
/// One retained outbound reliable packet. The stored <see cref="Packet"/> is rebuilt into a fresh
/// datagram verbatim on every attempt, which is what keeps the sequence number and ordering index
/// stable across a retransmission.
/// </summary>
internal sealed class PendingReliablePacket
{
    internal PendingReliablePacket(OutgoingPacket packet, ulong firstSentAtMilliseconds)
    {
        Packet = packet;
        FirstSentAtMilliseconds = firstSentAtMilliseconds;
        LastSentAtMilliseconds = firstSentAtMilliseconds;
    }

    /// <summary>The packet as originally built, including its split header when it is one part of a split.</summary>
    internal OutgoingPacket Packet { get; }

    /// <summary>When the first transmission went out. Kept for the give-up trace line.</summary>
    internal ulong FirstSentAtMilliseconds { get; }

    /// <summary>When the most recent transmission (first or retry) went out.</summary>
    internal ulong LastSentAtMilliseconds { get; private set; }

    /// <summary>Resends already performed. The first transmission does not count.</summary>
    internal int RetransmissionCount { get; private set; }

    /// <summary>Records one resend and restarts the timeout window.</summary>
    internal void RecordRetransmission(ulong nowMilliseconds)
    {
        this.RetransmissionCount++;
        this.LastSentAtMilliseconds = nowMilliseconds;
    }
}

/// <summary>Admission verdict for one inbound game packet.</summary>
public enum MtaAdmissionResult
{
    Drop,
    Deliver,
}

/// <summary>
/// Tunables for the four admission gates. The limits of the original server are not known, so
/// every limit here defaults to unlimited.
/// </summary>
public sealed class MtaAdmissionOptions
{
    /// <summary>
    /// Gate 1's per-IP, per-second allowance for mtaId 4 and 0x5a; 0 means unlimited. The reject
    /// threshold is <c>2 * limit</c>.
    /// </summary>
    public int Gate1RateLimit { get; set; }

    /// <summary>Gate 1 window length in milliseconds.</summary>
    public int Gate1WindowMs { get; set; } = 1000;

    /// <summary>Gate 2's per-id usage limits. 0 = unlimited.</summary>
    public int[] Gate2UsageLimits { get; } = new int[256];

    /// <summary>Gate 2's per-id bit budgets. 0 = unlimited.</summary>
    public int[] Gate2BitBudgets { get; } = new int[256];

    /// <summary>Gate 2's per-id packet-count budgets. 0 = unlimited.</summary>
    public ushort[] Gate2PacketBudgets { get; } = new ushort[256];

    /// <summary>
    /// Which client build this listener accepts. <see cref="MtaIdDialect.Release"/> (the default) is
    /// <c>net.dll</c>, <see cref="MtaIdDialect.Debug"/> is <c>net_d.dll</c>, and
    /// <see cref="MtaIdDialect.Both"/> / <see cref="MtaIdDialect.Auto"/> serve either build on one
    /// port.
    /// </summary>
    public MtaIdDialect PeerDialect { get; set; } = MtaIdDialect.Release;
}

/// <summary>
/// Stage 3 — the inbound admission policy.
///
/// This is a DISJUNCTION, not a conjunction:
///     gate2 != 0 AND (gate3 != 0 OR (gate4 != 0 AND whitelist(mtaId)))
/// A non-zero gate 3 delivers immediately, skipping gate 4 AND the whitelist.
/// </summary>
public static class MtaAdmission
{
    /// <summary>
    /// Minimum byte length of an inbound datagram on the game-packet path. This is a datagram-length
    /// test, not a payload-length test: <c>RID_DISCONNECTION_NOTIFICATION</c> is a legitimate 1-byte
    /// payload and the client sends it in a 20-byte datagram.
    /// </summary>
    public const int MinimumDatagramBytes = 8;

    /// <summary>
    /// The process-wide limiter behind the static <see cref="Gate1"/> shim and behind
    /// <see cref="Evaluate"/> when its caller does not supply one. <see cref="MtaNetWrapper"/> passes
    /// its own instance so that a server never shares a rate window with another wrapper.
    /// </summary>
    internal static Gate1RateLimiter SharedGate1 { get; } = new();

    /// <summary>
    /// Gate 1 — per-client-IP rate limit. Keyed on the client IP, not the port, and applied only to
    /// mtaId 4 and 0x5a. Returns true when the packet is allowed.
    ///
    /// <para>
    /// This overload uses <see cref="SharedGate1"/>; it exists for callers that have no wrapper
    /// instance (the gate is a pure policy function otherwise). Prefer a wrapper's own
    /// <see cref="Gate1RateLimiter"/> where one is available, because the shared table makes
    /// independent servers — and independent tests — contaminate each other.
    /// </para>
    /// </summary>
    public static bool Gate1(uint ipDword, byte mtaId, MtaAdmissionOptions options) =>
        SharedGate1.Allow(ipDword, mtaId, options);

    /// <summary>
    /// Gate 2 — per-id accumulation. Accumulates, then tests the usage limit, the bit budget and the
    /// packet-count budget. A rejection is a silent drop: no reply and no disconnect. Returns true
    /// when the packet is allowed.
    /// </summary>
    public static bool Gate2(
        MtaNetSession session, byte mtaId, int bitLength, MtaAdmissionOptions options)
    {
        // The flood-notice latch is both the notice latch and an entry short-circuit: once set,
        // everything from this player is rejected.
        if (session.FloodNoticeLatched)
            return false;

        if (session.InboundPacketCounts[mtaId] < 0xfffe)
            session.InboundPacketCounts[mtaId]++;

        session.InboundBitTotals[mtaId] += bitLength;

        int usageLimit = options.Gate2UsageLimits[mtaId];
        if (usageLimit != 0 && session.InboundPacketCounts[mtaId] > usageLimit)
        {
            session.FloodNoticeLatched = true;
            return false;
        }

        int bitBudget = options.Gate2BitBudgets[mtaId];
        if (bitBudget == 0 || session.InboundBitTotals[mtaId] <= bitBudget)
        {
            ushort countBudget = options.Gate2PacketBudgets[mtaId];
            if (countBudget == 0 || session.InboundPacketCounts[mtaId] <= countBudget)
                return true;
        }

        session.FloodNoticeLatched = true;
        return false;
    }

    /// <summary>
    /// Gate 3 — while the post-handshake latch is clear (the pre-handshake window) this admits only
    /// mtaId 3 and 4; once it is set it admits everything. Because a gate-3 pass bypasses the
    /// whitelist, this is the only reason a join can complete at all.
    /// </summary>
    public static bool Gate3(MtaNetSession session, byte mtaId)
    {
        if (!session.IsEstablished || session.Rejected || session.Flag854 || session.Flag855)
            return false;

        if (!session.HandshakeComplete)
            return mtaId == 3 || mtaId == 4;

        if (session.Counter97d < 2)
            return true;

        return session.Flag97f || !session.Flag97e;
    }

    /// <summary>
    /// Gate 4 — the exact complement of gate 3's post-handshake arm.
    /// </summary>
    public static bool Gate4(MtaNetSession session)
    {
        return session.IsEstablished
            && !session.Rejected
            && !session.Flag854
            && !session.Flag855
            && session.HandshakeComplete
            && session.Counter97d > 1
            && !session.Flag97f
            && session.Flag97e;
    }

    /// <summary>
    /// The full policy for one inbound game packet, in gate order.
    /// </summary>
    /// <param name="session">The source session record.</param>
    /// <param name="wireId">The raw wire packet id byte.</param>
    /// <param name="payload">The packet bytes, starting at the wire id.</param>
    /// <param name="bitLength">The packet's bit length (byte length * 8 for byte-aligned payloads).</param>
    /// <param name="options">Gate tunables.</param>
    /// <param name="mtaId">The resolved 0-based MTA packet id.</param>
    /// <param name="reason">A short explanation of the verdict, for logging.</param>
    /// <param name="gate1">
    /// Gate 1's rate table. Omit to use the process-wide <see cref="SharedGate1"/>; a wrapper passes
    /// its own so that two servers, or two tests, cannot contaminate each other's rate window.
    /// </param>
    public static MtaAdmissionResult Evaluate(
        MtaNetSession session,
        byte wireId,
        byte[] payload,
        int bitLength,
        MtaAdmissionOptions options,
        out byte mtaId,
        out string reason,
        Gate1RateLimiter? gate1 = null)
    {
        mtaId = 0;
        reason = string.Empty;

        if (wireId == 0xff)
        {
            reason = "wire id 0xff";
            return MtaAdmissionResult.Drop;
        }

        int resolved = wireId - MtaProtocol.MtaIdToWireId;
        if (resolved < 0)
        {
            // A sub-99 id is a RakNet protocol message, not a game packet; those are handled by the
            // connected-message dispatcher before the game-packet path. The wrapper routes them away, so
            // this branch is unreachable in normal operation and exists only so a direct caller cannot
            // smuggle one past the gates.
            reason = $"RakNet protocol id {wireId} ({MtaProtocol.RidName(wireId)}), not a game packet";
            return MtaAdmissionResult.Drop;
        }

        // The 4 <-> 0x13 remap is the single point where the two shipped builds disagree. Release
        // (net.dll) applies it, debug (net_d.dll) does not, and MtaIdDialect picks which behaviour this
        // listener implements. It happens before any gate, because every gate below keys on the
        // server-side mtaId.
        MtaIdDialect dialect = options.PeerDialect;
        if (dialect == MtaIdDialect.Auto)
        {
            // Latch from the first id that can only mean one build. A joining client's first game
            // packet is its join data, and the two builds put that on different wire ids, so the first
            // such packet is unambiguous.
            if (session.DetectedDialect is null && (resolved == 4 || resolved == 0x13))
            {
                session.DetectedDialect = resolved == 0x13 ? MtaIdDialect.Release : MtaIdDialect.Debug;
            }

            dialect = session.DetectedDialect ?? MtaIdDialect.Both;
        }

        byte id = MtaProtocol.ResolveInboundId((byte)resolved, dialect);
        if (id == 0 || id == 0xff)
        {
            reason = $"mta id {id}";
            return MtaAdmissionResult.Drop;
        }

        mtaId = id;

        uint ipDword = (uint)(session.BinaryAddress >> 32);
        if (!(gate1 ?? SharedGate1).Allow(ipDword, id, options))
        {
            reason = "gate 1 (connection rate limit)";
            return MtaAdmissionResult.Drop;
        }

        if (!Gate2(session, id, bitLength, options))
        {
            reason = "gate 2 (per-id budget)";
            return MtaAdmissionResult.Drop;
        }

        // Gate 3 is a bypass: a pass skips gate 4 AND the whitelist. It does not skip the join-data
        // version gate below.
        if (!Gate3(session, id))
        {
            if (!Gate4(session))
            {
                reason = "gates 3 and 4 both denied";
                return MtaAdmissionResult.Drop;
            }

            if (!MtaProtocol.IsWhitelisted(id, payload, bitLength))
            {
                reason = $"mta id {id} not whitelisted";
                return MtaAdmissionResult.Drop;
            }
        }

        // The one receive-side version gate, applied to the post-dialect id: id 4 (join data; wire id
        // 0x76 from a release client, 0x67 from a debug client) is valid only before a bitstream version
        // has been negotiated for the connection. The negotiated version is set when the join data is
        // processed, so a repeat join-data packet on the same connection is silently dropped like any
        // other gate failure.
        if (id == 4 && session.NegotiatedBitStreamVersion != 0)
        {
            reason = "mta id 4 with a negotiated bitstream version";
            return MtaAdmissionResult.Drop;
        }

        return MtaAdmissionResult.Deliver;
    }
}
