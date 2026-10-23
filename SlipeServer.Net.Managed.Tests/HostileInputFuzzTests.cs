using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SlipeServer.Net.Managed;
using SlipeServer.Net.Wrappers;
using SlipeServer.Packets.Enums;
using Xunit;
using Xunit.Abstractions;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Deterministic, structure-aware fuzzing of everything reachable from one received datagram.
///
/// <para>
/// <see cref="MtaNetWrapper.ReceiveLoopAsync"/> already wraps <see cref="MtaNetWrapper.InjectDatagram"/>
/// in a catch, so a malformed datagram cannot kill the receive loop. That catch is a belt, not the
/// braces: this file is the argument that no datagram can make it necessary. The properties asserted
/// here are the five the receive loop cannot give on its own — no exception escapes the parser, no
/// wire-declared length is trusted for an allocation, no per-peer state grows without bound, a later
/// valid datagram from the same peer still delivers, and a second peer is untouched.
/// </para>
///
/// <para>
/// The corpus is independent wire literals only: <c>capture.log</c> records, the hand-transcribed
/// <see cref="CaptureVectors"/>, and the four real split parts. Nothing here is produced by
/// <c>RakNetDatagramWriter</c> or any other part of the port, because a round trip through the port's
/// own encoder would prove internal consistency and hide exactly the framing mistakes this is meant
/// to find. The mutations are structural — truncation, single-bit walk, aligned length/count field
/// overwrite, record duplication and omission — because uniform noise mostly fails the same length
/// check and never reaches the parsers behind it.
/// </para>
///
/// <para>
/// Every mutation is deterministic and its name is printed on failure, so a failure is reproducible
/// from the report alone. The genuinely slow sweep is behind <c>MTANET_FUZZ=1</c>, in the style of
/// <see cref="ProbeFactAttribute"/>.
/// </para>
/// </summary>
public sealed class HostileInputFuzzTests
{
    private readonly ITestOutputHelper output;

    public HostileInputFuzzTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    /// <summary>
    /// How the post-sweep valid datagram behaved, per seed. Reported rather than asserted: the
    /// interesting numbers are "the strict path was exercised" and "the documented deviations only
    /// happened where the peer caused them itself".
    /// </summary>
    private int deliveredStrict;

    private int dedupedAsRepeat;

    private int heldBehindHole;

    private int staleSlot;
    // ================================================================ the corpus

    private sealed record FuzzSeed(string Name, byte[] Datagram);

    /// <summary>
    /// capture.log record 5 — the real client's first ack-only datagram
    /// (<c>datagram acks=1 pkts=0 second=False</c>). Transcribed from the recording.
    /// </summary>
    private const string AckOnlyShort = "8000000000000000630000000000";

    /// <summary>
    /// capture.log record 29 — another ack-only datagram from the same session
    /// (<c>datagram acks=1 pkts=0 second=False</c>), four bytes longer than
    /// <see cref="AckOnlyShort"/>, i.e. the one recorded ack block that carries more than a single
    /// entry. Transcribed from the recording.
    /// </summary>
    private const string AckOnlyLong = "800000000000000062050000000600000000";

    private static readonly IPEndPoint PeerA = WrapperHarness.CaptureLogPeer;

    private static readonly IPEndPoint PeerB = WireTestKit.Endpoint("10.0.0.7:1234");

    private static readonly IPEndPoint PeerC = WireTestKit.Endpoint("10.0.0.9:4321");

    /// <summary>
    /// A distinct address per seed. Each seed's sweep therefore starts from a virgin session, so a
    /// mutation in seed <c>i</c> cannot make seed <c>i+1</c> look clean (or dirty), and the strict
    /// "the recorded valid datagram still delivers" path is exercised once per seed instead of once
    /// per run.
    /// </summary>
    private static IPEndPoint AttackerPeer(int seedIndex) =>
        WireTestKit.Endpoint($"10.1.0.{seedIndex + 1}:{51000 + seedIndex}");

    /// <summary>
    /// Seeds whose recorded meaning is "this session continues". A datagram from this corpus must
    /// leave the sender's session usable and the other peers untouched.
    /// </summary>
    private static readonly FuzzSeed[] Corpus = BuildCorpus();

    /// <summary>
    /// The one recorded datagram whose job is to end the session. It is swept separately because
    /// "the session survives" is the wrong property for it; "only the sender's session may end, and
    /// the sender may immediately reconnect" is the right one.
    /// </summary>
    private static readonly FuzzSeed[] TearDownCorpus =
    {
        Seed("connected/disconnection-notification", CaptureVectors.DisconnectionNotification),
    };

    private static FuzzSeed[] BuildCorpus()
    {
        var seeds = new List<FuzzSeed>
        {
            // The offline surface: unauthenticated by definition, so anyone who knows the port can
            // send these before any handshake exists.
            Seed("offline/open-connection-request", CaptureVectors.OpenConnectionRequest),
            Seed("offline/open-connection-bad-version", CaptureVectors.OpenConnectionRequestBadVersion),
            Seed("offline/connected-connection-request", CaptureVectors.ConnectedConnectionRequest),
            Seed("offline/ping", CaptureVectors.PingRequest),

            // The connected surface.
            Seed("connected/incoming-connection+system", CaptureVectors.IncomingConnectionAndSystemPacket),
            Seed("connected/rpc-wire-83", CaptureVectors.RpcPacketWire83),
            Seed("connected/rpc-wire-7b", CaptureVectors.RpcPacketWire7B),
            Seed("connected/two-wire-7b-packets", CaptureVectors.TwoWire7BPackets),
            Seed("connected/server-join-complete", CaptureVectors.ServerJoinCompleteStock),
            Seed("connected/text-packet", CaptureVectors.TextPacketStock),

            // The ack block: the only recorded datagrams whose body is nothing but acks.
            Seed("acks/ack-only-short", AckOnlyShort),
            Seed("acks/ack-only-long", AckOnlyLong),
        };

        for (int i = 0; i < SplitReassemblyTests.RecordedSplitParts.Length; i++)
            seeds.Add(Seed($"split/part-{i}", SplitReassemblyTests.RecordedSplitParts[i]));

        return seeds.ToArray();
    }

    private static FuzzSeed Seed(string name, string hex) => new(name, WireTestKit.Hex(hex));

    // ================================================================ the mutations

    private readonly struct Mutation
    {
        internal Mutation(string name, byte[] datagram)
        {
            this.Name = name;
            this.Datagram = datagram;
        }

        internal string Name { get; }

        internal byte[] Datagram { get; }
    }

    /// <summary>16-bit values a length, count or index field is replaced with, one at a time.</summary>
    private static readonly ushort[] SixteenBitFields = { 0x0000, 0x0001, 0x7FFF, 0xFFFF };

    /// <summary>32-bit values a sequence number, ordering index or count field is replaced with.</summary>
    private static readonly uint[] ThirtyTwoBitFields = { 0x00000000, 0x00000001, 0x7FFFFFFF, 0xFFFFFFFF };

    /// <summary>
    /// The fixed seed list behind the splice mutations. Drawn from a literal list, not from the clock,
    /// so the same corpus always produces the same splices and a reported seed reproduces the case.
    /// </summary>
    internal static readonly int[] SpliceSeeds = { 1, 7, 13, 29, 47, 71, 101, 197 };

    private const int SplicesPerSeed = 6;

    /// <summary>
    /// Every structural mutation of one seed datagram. The order is fixed and the names are unique, so
    /// a reported failure identifies exactly one case.
    /// </summary>
    private static IEnumerable<Mutation> Mutations(FuzzSeed seed)
    {
        byte[] good = seed.Datagram;

        // Every prefix, including the empty datagram and the two lengths below InjectDatagram's own
        // 3-byte floor: a prefix is what a short UDP read looks like.
        for (int n = 0; n <= good.Length; n++)
            yield return new Mutation($"truncate {n}/{good.Length}", good[..n]);

        // Single-bit walk. This is the one mutation class no single length check can absorb, so it is
        // exhaustive rather than sampled.
        for (int i = 0; i < good.Length; i++)
        {
            for (int bit = 0; bit < 8; bit++)
            {
                byte[] copy = (byte[])good.Clone();
                copy[i] ^= (byte)(0x80 >> bit);
                yield return new Mutation($"flip bit {bit} of byte {i}", copy);
            }
        }

        // Every aligned 16-bit window treated as a length or count field. The offsets are not derived
        // from the layout on purpose: a walk over all aligned words covers the ack range count, the
        // split index and count and the per-packet bit count without this file restating them, and it
        // cannot go stale when a vector changes.
        foreach (ushort value in SixteenBitFields)
        {
            for (int i = 0; i + 1 < good.Length; i += 2)
            {
                byte[] copy = (byte[])good.Clone();
                copy[i] = (byte)value;
                copy[i + 1] = (byte)(value >> 8);
                yield return new Mutation($"word {i} = 0x{value:X4}", copy);
            }
        }

        // The same walk over aligned 32-bit windows, for the sequence numbers, ordering indexes and
        // the ack block's 32-bit range ends.
        foreach (uint value in ThirtyTwoBitFields)
        {
            for (int i = 0; i + 3 < good.Length; i += 4)
            {
                byte[] copy = (byte[])good.Clone();
                copy[i] = (byte)value;
                copy[i + 1] = (byte)(value >> 8);
                copy[i + 2] = (byte)(value >> 16);
                copy[i + 3] = (byte)(value >> 24);
                yield return new Mutation($"dword {i} = 0x{value:X8}", copy);
            }
        }

        // The same datagram twice in one buffer, and one copy with a tail appended: a peer that
        // repeats a record without the reliability layer's sequence dedupe noticing.
        yield return new Mutation("doubled", [.. good, .. good]);
        yield return new Mutation("tail 0xFF", [.. good, 0xFF]);
        yield return new Mutation("tail 0x00 x8", [.. good, 0, 0, 0, 0, 0, 0, 0, 0]);

        // Seeded splices: duplicate or omit a byte range. This is how a record in the ack block or in
        // the packet-record loop gets repeated or dropped without this file needing to know where the
        // record boundaries are.
        foreach (int spliceSeed in SpliceSeeds)
        {
            var random = new Random(spliceSeed);
            for (int k = 0; k < SplicesPerSeed; k++)
            {
                if (good.Length < 2)
                    break;

                int start = random.Next(good.Length);
                int length = 1 + random.Next(Math.Min(8, good.Length - start));
                int end = start + length;
                bool duplicate = random.Next(2) == 0;

                byte[] spliced = duplicate
                    ? [.. good[..end], .. good[start..end], .. good[end..]]
                    : [.. good[..start], .. good[end..]];

                yield return new Mutation(
                    $"{(duplicate ? "duplicate" : "omit")} bytes {start}..{end} (splice seed {spliceSeed})",
                    spliced);
            }
        }
    }

    /// <summary>
    /// The opt-in deep pass: a seeded random byte walk over the corpus, many times the default
    /// budget. It is a superset of nothing — the structural sweep is the one that has to be
    /// exhaustive — but it reaches combinations the structural mutations cannot, e.g. two fields
    /// wrong at once.
    /// </summary>
    private static IEnumerable<Mutation> RandomMutations(FuzzSeed seed, int rounds)
    {
        foreach (int spliceSeed in SpliceSeeds)
        {
            var random = new Random(spliceSeed * 31 + seed.Datagram.Length);
            for (int round = 0; round < rounds; round++)
            {
                byte[] copy = (byte[])seed.Datagram.Clone();
                int edits = 1 + random.Next(6);
                for (int e = 0; e < edits && copy.Length > 0; e++)
                    copy[random.Next(copy.Length)] = (byte)random.Next(256);

                yield return new Mutation($"random walk round {round} (splice seed {spliceSeed})", copy);
            }
        }
    }

    // ================================================================ the properties

    /// <summary>
    /// How much heap one datagram may cost before the parser has taken a wire field at its word.
    ///
    /// <para>
    /// A parse is linear in the datagram: one <see cref="BitStream"/> copy, one payload array, one
    /// record object per packet. 64 bytes of heap per wire byte is already several times that. The
    /// 16 KiB floor is scaffolding, not headroom for the parser — it absorbs first-call JIT and the
    /// outbound reply a handler may build, neither of which scales with the input. The bound this
    /// enforces is therefore "no single datagram can buy a large buffer from a length field": a
    /// declared <c>splitPacketCount</c> of 0x4000 costing 128 KiB is caught for every seed up to
    /// roughly 1.7 KiB, and the exact claims behind that are pinned separately by
    /// <see cref="SplitReassemblyDoesNotSizeItsPartTableFromTheDeclaredCount"/> and
    /// <see cref="ReadLengthFieldsCannotBuyBuffersLargerThanTheStream"/>.
    /// </para>
    /// </summary>
    private static long AllocationBudget(int datagramLength) => Math.Max(datagramLength * 64, 1024) + 16384;

    /// <summary>
    /// How many datagrams one wrapper may see before it is recycled. Chosen so a bounded container
    /// (the sequence dedupe set at 4096, the in-flight reliable ledger at 1024) stays far from its cap
    /// and therefore never reallocates inside a single iteration's measurement. See
    /// <see cref="FuzzEnd.Recycle"/>.
    /// </summary>
    private const int RoundsPerWrapper = 512;

    /// <summary>
    /// Recycles the wrapper and re-establishes the peers the sweep needs, then declares the ordering
    /// precondition from the unmutated datagram.
    ///
    /// <para>
    /// Declaring it once per restart rather than per iteration is deliberate: doing it per iteration
    /// would let a mutation that rewrites an ordering index move the channel somewhere no real peer
    /// could and starve the rest of the sweep of deliveries, while not doing it at all would leave
    /// every recorded slice held behind a hole the recording already filled, so the game-packet
    /// parsers would never be reached.
    /// </para>
    ///
    /// <para>Returns the number of live sessions, so the caller can assert nothing was conjured.</para>
    /// </summary>
    private static int Restart(FuzzEnd end, FuzzSeed seed, IPEndPoint attacker)
    {
        end.Recycle();
        Establish(end, attacker);
        Establish(end, PeerB);
        Establish(end, PeerC);
        WrapperHarness.PrimeOrdering(end.Wrapper, seed.Datagram, attacker);
        return 3;
    }

    /// <summary>
    /// The whole structural corpus, in one pass, with every property asserted. One test rather than
    /// several because the sweep is the expensive part; each assertion names the seed and mutation it
    /// failed on, so a single test is not a loss of diagnosability.
    /// </summary>
    [Fact]
    public void NoHostileDatagramEscapesInjectDatagramOrOverAllocates()
    {
        using var end = new FuzzEnd();

        int iterations = 0;
        long totalAllocated = 0;
        long totalBytes = 0;
        int seedIndex = 0;

        foreach (FuzzSeed seed in Corpus)
        {
            // A fresh sender per seed, so no seed's mutations can make a later seed's sweep look
            // clean, and every seed gets a virgin session to prove "a valid datagram still delivers"
            // against. Peers B and C are never sent anything, so if their session disappears the
            // assertion says so instead of being repaired here.
            IPEndPoint attacker = AttackerPeer(seedIndex++);
            int expectedSessions = Restart(end, seed, attacker);
            int sinceRestart = 0;

            foreach (Mutation mutation in Mutations(seed))
            {
                if (sinceRestart == RoundsPerWrapper)
                {
                    expectedSessions = Restart(end, seed, attacker);
                    sinceRestart = 0;
                }

                sinceRestart++;
                iterations++;
                totalBytes += mutation.Datagram.Length;

                long before = GC.GetAllocatedBytesForCurrentThread();
                try
                {
                    end.Wrapper.InjectDatagram(mutation.Datagram, mutation.Datagram.Length, attacker);
                }
                catch (Exception ex)
                {
                    Assert.Fail(
                        $"seed '{seed.Name}', mutation '{mutation.Name}': InjectDatagram threw " +
                        $"{ex.GetType().Name}: {ex.Message}\n" +
                        $"  datagram ({mutation.Datagram.Length} B): {WireTestKit.HexOf(mutation.Datagram)}");
                }

                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                totalAllocated += allocated;

                // The harness's own bookkeeping must not be attributed to the parser: a delivery
                // appends to this list, and an unchecked list would eventually reallocate mid-
                // measurement and look like a parser over-allocation.
                end.Events.Clear();

                long budget = AllocationBudget(mutation.Datagram.Length);
                if (allocated > budget)
                {
                    Assert.Fail(
                        $"seed '{seed.Name}', mutation '{mutation.Name}': one {mutation.Datagram.Length}-byte " +
                        $"datagram allocated {allocated} B, over the {budget} B budget.\n" +
                        $"  datagram: {WireTestKit.HexOf(mutation.Datagram)}");
                }
            }

            AssertNothingGrewPastItsCap(end, seed, attacker, expectedSessions);
            AssertSessionStillDelivers(end, seed, attacker, allowDocumentedDeviation: true);
            AssertUntouchedPeersStillHaveSessions(end, seed);
        }

        AssertUntouchedPeersStillDeliver(end, Corpus[^1]);

        Assert.True(iterations >= 50_000, $"the structural sweep only produced {iterations} cases");
        Assert.True(totalBytes > 0 && totalAllocated > 0, "the sweep did not run");

        this.output.WriteLine(
            $"structural sweep: {iterations} datagrams over {Corpus.Length} seeds, " +
            $"{totalBytes} wire bytes, {totalAllocated} B allocated; " +
            $"post-sweep valid datagram: {this.deliveredStrict} delivered, " +
            $"{this.dedupedAsRepeat} deduped as a repeat, {this.staleSlot} stale ordering slots, " +
            $"{this.heldBehindHole} held behind an ordering hole");
        Assert.True(
            this.deliveredStrict + this.dedupedAsRepeat + this.staleSlot + this.heldBehindHole ==
                Corpus.Length + 2,
            "every seed and both untouched peers must reach exactly one post-sweep outcome");
        Assert.True(
            this.deliveredStrict >= Corpus.Length / 2,
            $"only {this.deliveredStrict} of {Corpus.Length} seeds took the strict delivery path; " +
            "the sweep is not exercising the parsers it claims to");
    }

    /// <summary>
    /// A real <c>RID_DISCONNECTION_NOTIFICATION</c> is supposed to end a session, so it cannot share a
    /// sweep with the property "the session survives". What it must not do is reach a session other
    /// than the sender's, and what its own session must not become is unusable: a peer that
    /// disconnects and immediately reconnects has to be able to play.
    /// </summary>
    [Fact]
    public void AHostileDisconnectionOnlyEverTearsDownTheSendersOwnSession()
    {
        using var end = new FuzzEnd();
        Establish(end, PeerA);
        Establish(end, PeerB);
        Establish(end, PeerC);

        int iterations = 0;
        foreach (FuzzSeed seed in TearDownCorpus)
        {
            WrapperHarness.PrimeOrdering(end.Wrapper, seed.Datagram, PeerA);

            foreach (Mutation mutation in Mutations(seed))
            {
                iterations++;
                try
                {
                    end.Wrapper.InjectDatagram(mutation.Datagram, mutation.Datagram.Length, PeerA);
                }
                catch (Exception ex)
                {
                    Assert.Fail(
                        $"seed '{seed.Name}', mutation '{mutation.Name}': InjectDatagram threw " +
                        $"{ex.GetType().Name}: {ex.Message}\n" +
                        $"  datagram ({mutation.Datagram.Length} B): {WireTestKit.HexOf(mutation.Datagram)}");
                }
            }

            // The datagrams came from peer A, so only peer A's session may be gone.
            AssertUntouchedPeersStillDeliver(end, seed);

            // Peer A may or may not still be connected. If the sweep disconnected it, reconnecting has
            // to give it a working session — strictly, with no allowance. If it survived, the only
            // outcomes allowed are the two documented ones.
            bool reconnected = SessionOrNull(end, PeerA) is null;
            if (reconnected)
                Establish(end, PeerA);

            AssertSessionStillDelivers(end, seed, PeerA, allowDocumentedDeviation: !reconnected);
        }

        Assert.True(iterations > 100, $"the teardown sweep only produced {iterations} cases");
    }

    /// <summary>
    /// The opt-in deep pass. Same properties, a seeded random walk instead of the structural sweep,
    /// and many more cases than the default run can afford.
    /// </summary>
    [FuzzFact]
    public void RandomWalkDoesNotEscapeInjectDatagramOrOverAllocate()
    {
        using var end = new FuzzEnd();

        int iterations = 0;
        int seedIndex = 0;
        foreach (FuzzSeed seed in Corpus)
        {
            IPEndPoint attacker = AttackerPeer(seedIndex++);
            int expectedSessions = Restart(end, seed, attacker);
            int sinceRestart = 0;

            foreach (Mutation mutation in RandomMutations(seed, rounds: 512))
            {
                if (sinceRestart == RoundsPerWrapper)
                {
                    expectedSessions = Restart(end, seed, attacker);
                    sinceRestart = 0;
                }

                sinceRestart++;
                iterations++;
                long before = GC.GetAllocatedBytesForCurrentThread();
                try
                {
                    end.Wrapper.InjectDatagram(mutation.Datagram, mutation.Datagram.Length, attacker);
                }
                catch (Exception ex)
                {
                    Assert.Fail(
                        $"seed '{seed.Name}', mutation '{mutation.Name}': InjectDatagram threw " +
                        $"{ex.GetType().Name}: {ex.Message}\n" +
                        $"  datagram ({mutation.Datagram.Length} B): {WireTestKit.HexOf(mutation.Datagram)}");
                }

                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                end.Events.Clear();

                long budget = AllocationBudget(mutation.Datagram.Length);
                if (allocated > budget)
                {
                    Assert.Fail(
                        $"seed '{seed.Name}', mutation '{mutation.Name}': one {mutation.Datagram.Length}-byte " +
                        $"datagram allocated {allocated} B, over the {budget} B budget.\n" +
                        $"  datagram: {WireTestKit.HexOf(mutation.Datagram)}");
                }
            }

            AssertNothingGrewPastItsCap(end, seed, attacker, expectedSessions);
            AssertSessionStillDelivers(end, seed, attacker, allowDocumentedDeviation: true);
            AssertUntouchedPeersStillHaveSessions(end, seed);
        }

        AssertUntouchedPeersStillDeliver(end, Corpus[^1]);

        this.output.WriteLine($"random walk: {iterations} datagrams over {Corpus.Length} seeds");

        Assert.True(iterations >= 50_000, $"the random walk only produced {iterations} cases");
    }

    // ================================================================ the precise claims

    /// <summary>
    /// A declared part count must never size storage. The parser's sanity cap is 0x4000 parts, so an
    /// array indexed by it would turn one short datagram into 128 KiB, and
    /// <see cref="SplitReassembler.MaxPendingMessages"/> of those into 4 MiB for one peer.
    /// </summary>
    [Fact]
    public void SplitReassemblyDoesNotSizeItsPartTableFromTheDeclaredCount()
    {
        var reassembler = new SplitReassembler();
        var part = new RakNetPacketRecord
        {
            SequenceNumber = 1,
            Reliability = 3,
            OrderingChannel = 0,
            OrderingIndex = 0,
            SplitPacketId = 7,
            SplitPacketIndex = 0,
            SplitPacketCount = 0x4000,
            BitCount = 8,
            Payload = [0x5A],
        };

        long before = GC.GetAllocatedBytesForCurrentThread();
        Assert.Equal(SplitAcceptance.Buffered, reassembler.Accept(part, 0, out _));
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(1, reassembler.PendingMessageCount);
        Assert.True(
            allocated < 8 * 1024,
            $"one 20-byte split part declaring 0x4000 parts allocated {allocated} B");
    }

    /// <summary>
    /// The per-peer sequence dedupe set is a wire-driven container, so it has to be bounded: a peer
    /// that invents a new sequence number per datagram would otherwise grow it for the life of the
    /// session, and a datagram is 20 bytes.
    /// </summary>
    [Fact]
    public void ReceivedSequenceTrackingIsBoundedAndStillDedupes()
    {
        var session = new MtaNetSession(0x1234, PeerA);

        for (uint sequence = 0; sequence < 100_000; sequence++)
            Assert.True(session.TrackReceivedSequence(sequence));

        Assert.True(
            session.ReceivedSequences.Count <= MtaNetSession.MaxTrackedReceivedSequences,
            $"{session.ReceivedSequences.Count} entries tracked");

        // A retransmission of something inside the window is still caught.
        Assert.False(session.TrackReceivedSequence(99_999));
        Assert.False(session.TrackReceivedSequence(99_000));
    }

    /// <summary>
    /// The three <see cref="BitStream"/> read paths that can be told a length: a string length, a
    /// bit count and a destination buffer. None of them may allocate from the wire value before
    /// checking it against what is left, and none may write past the caller's buffer.
    /// </summary>
    [Fact]
    public void ReadLengthFieldsCannotBuyBuffersLargerThanTheStream()
    {
        // A 4-byte stream whose first 32 bits are the length 0x7FFFFFFF: the length itself is the
        // whole stream, so every read below has to fail rather than allocate from it.
        var stream = new BitStream(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, 0, 4);

        Assert.False(stream.ReadStr(out _));
        Assert.False(stream.ReadString(out _));

        stream = new BitStream(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, 0, 4);
        Assert.False(stream.ReadString(out _));

        // A caller's buffer smaller than the requested bit count is refused, not overrun.
        var small = new byte[1];
        Assert.False(new BitStream(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }, 0, 4).ReadBits(small, 64, true));
        Assert.False(new BitStream(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }, 0, 4).ReadCompressed(small, 64, true));

        // And the ordinary case still works, so the guard is not simply "always false".
        var exact = new byte[4];
        Assert.True(new BitStream(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }, 0, 4).ReadBits(exact, 32, true));
        Assert.Equal(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }, exact);
    }

    // ================================================================ plumbing

    /// <summary>
    /// A wrapper with no socket, no capture file and no trace sink. The capture file would put a disk
    /// write on every iteration and a trace sink would allocate one string per datagram, both of
    /// which would swamp the allocation measurement that is the point of the harness.
    /// </summary>
    private sealed class FuzzEnd : IDisposable
    {
        internal FuzzEnd() => this.Wrapper = this.CreateWrapper();

        internal MtaNetWrapper Wrapper { get; private set; }

        internal List<Received> Events { get; } = new();

        /// <summary>
        /// Replaces the wrapper with a fresh one, so a long sweep does not measure the amortised cost of
        /// a container resize against a single datagram.
        ///
        /// <para>
        /// The session containers are bounded, not constant: the sequence dedupe set reaches
        /// <see cref="MtaNetSession.MaxTrackedReceivedSequences"/> and the in-flight reliable ledger
        /// reaches <see cref="MtaNetSession.MaxUnackedReliablePackets"/>, and a dictionary or hash set
        /// reallocates as it fills. That reallocation is legitimate and amortised, but it lands inside
        /// one iteration's measurement and would be reported as "this 48-byte datagram allocated
        /// 30 KiB". Recycling keeps every measurement attributable to the datagram that caused it.
        /// </para>
        /// </summary>
        internal void Recycle()
        {
            this.Wrapper.Dispose();
            this.Wrapper = this.CreateWrapper();
            this.Events.Clear();
        }

        public void Dispose() => this.Wrapper.Dispose();

        private MtaNetWrapper CreateWrapper()
        {
            var wrapper = new MtaNetWrapper();
            wrapper.PacketReceived += (sender, address, id, data, ping) =>
                this.Events.Add(new Received(sender, address, id, data, ping));
            return wrapper;
        }
    }

    /// <summary>Completes the offline handshake for one peer and clears the event log.</summary>
    private static void Establish(FuzzEnd end, IPEndPoint peer)
    {
        byte[] open = CaptureVectors.OpenRequestBytes;
        byte[] connect = CaptureVectors.ConnectedRequestBytes;

        end.Wrapper.InjectDatagram(open, open.Length, peer);
        end.Wrapper.InjectDatagram(connect, connect.Length, peer);
        end.Events.Clear();
    }

    /// <summary>
    /// Property 2's session half: a wire field must not be able to buy a container that keeps
    /// growing. Each of these is the cap the container itself enforces, so the assertion is that the
    /// sweep did not find a path around it.
    /// </summary>
    private static void AssertNothingGrewPastItsCap(
        FuzzEnd end, FuzzSeed seed, IPEndPoint attacker, int expectedSessions)
    {
        MtaNetSession? sessionA = SessionOrNull(end, attacker);
        Assert.True(sessionA is not null, $"seed '{seed.Name}': the sweep destroyed the sender's session");

        Assert.True(
            sessionA!.ReceivedSequences.Count <= MtaNetSession.MaxTrackedReceivedSequences,
            $"seed '{seed.Name}': the sender's sequence dedupe set reached " +
            $"{sessionA.ReceivedSequences.Count}, over the {MtaNetSession.MaxTrackedReceivedSequences} cap");
        Assert.True(
            sessionA.InboundSplits.PendingMessageCount <= SplitReassembler.MaxPendingMessages,
            $"seed '{seed.Name}': the sender buffered {sessionA.InboundSplits.PendingMessageCount} incomplete " +
            $"split messages, over the {SplitReassembler.MaxPendingMessages} cap");
        Assert.True(
            sessionA.InboundOrdering.HeldRecordCount <=
                InboundOrdering.MaxPendingMessagesPerChannel * InboundOrdering.MaxOrderingChannels,
            $"seed '{seed.Name}': the sender held {sessionA.InboundOrdering.HeldRecordCount} ordering records, " +
            "over the per-channel cap");
        Assert.True(
            sessionA.UnackedReliableCount <= MtaNetSession.MaxUnackedReliablePackets,
            $"seed '{seed.Name}': the sender accumulated {sessionA.UnackedReliableCount} unacked reliable " +
            $"packets, over the {MtaNetSession.MaxUnackedReliablePackets} cap");

        // Garbage must not conjure a session (that would need a handshake) and must not reap one.
        Assert.True(
            end.Wrapper.SessionCount == expectedSessions,
            $"seed '{seed.Name}': {end.Wrapper.SessionCount} sessions live, expected {expectedSessions}");
    }

    /// <summary>
    /// Property 4: after the sweep, a recorded valid datagram from one peer still behaves.
    ///
    /// <para>
    /// The strict form of the property is "delivers exactly once, to the peer that sent it". Three
    /// other outcomes are correct protocol behaviour rather than parser defects, and only the peer
    /// that sent the garbage may show them:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <b>Re-acked, not re-delivered</b> — a mutation that kept the same sequence number already
    /// consumed this packet, so the reliability layer is right to drop the repeat. This is the
    /// behaviour <c>DuplicateSequenceNumbersAreReAckedButNotReDelivered</c> pins on purpose.
    /// </item>
    /// <item>
    /// <b>Held behind a hole</b> — a mutation declared a larger ordering index on the peer's own
    /// channel, so the channel is now waiting for an index the peer never sent. That is what the wire
    /// protocol means; the peer did it to itself, it is bounded by
    /// <see cref="InboundOrdering.MaxPendingMessagesPerChannel"/>, and it cannot reach another peer.
    /// </item>
    /// <item>
    /// <b>Stale ordering slot</b> — a mutation delivered some other record into the same ordering slot
    /// this datagram claims, so the channel has already passed it and the ordering layer is right to
    /// drop the newcomer as stale. Again the peer did it to itself.
    /// </item>
    /// </list>
    ///
    /// <para>
    /// <see cref="WrapperHarness.PrimeOrdering"/> stands in for the packets the recording had already
    /// delivered before this slice; it only ever moves a channel index forward, so it cannot repair
    /// state the sweep pushed somewhere a peer could not have pushed it.
    /// </para>
    /// </summary>
    private void AssertSessionStillDelivers(
        FuzzEnd end, FuzzSeed seed, IPEndPoint peer, bool allowDocumentedDeviation)
    {
        MtaNetSession session = Session(end, peer);
        bool alreadyConsumed = RpcWire83Sequences().All(
            sequence => session.ReceivedSequences.Contains(sequence));
        int heldBefore = session.InboundOrdering.HeldRecordCount;
        (byte channel, uint orderedIndex) = RpcWire83OrderingSlot();
        bool slotAlreadyDelivered =
            session.InboundOrdering.ExpectedOrderedIndexFor(channel) > orderedIndex;

        end.Events.Clear();
        WrapperHarness.PrimeOrdering(end.Wrapper, CaptureVectors.RpcPacketWire83Bytes, peer);
        end.Wrapper.InjectDatagram(
            CaptureVectors.RpcPacketWire83Bytes, CaptureVectors.RpcPacketWire83Bytes.Length, peer);

        if (end.Events.Count == 1)
        {
            deliveredStrict++;
            Received received = end.Events[0];
            Assert.True(
                received.Id == PacketId.PACKET_ID_PLAYER_PURESYNC,
                $"seed '{seed.Name}' from {peer}: delivered id {received.Id}, not PACKET_ID_PLAYER_PURESYNC");
            Assert.True(
                WireTestKit.HexOf(received.Data) == "0000000000180000000000000000000001006400008807D13B0000",
                $"seed '{seed.Name}' from {peer}: delivered payload {WireTestKit.HexOf(received.Data)}");
            Assert.Equal(MtaNetWrapper.BinaryAddressOf(peer), received.Address);
        }
        else
        {
            bool held = session.InboundOrdering.HeldRecordCount > heldBefore;
            if (alreadyConsumed)
                dedupedAsRepeat++;
            else if (slotAlreadyDelivered)
                staleSlot++;
            else
                heldBehindHole++;

            Assert.True(
                allowDocumentedDeviation && (alreadyConsumed || slotAlreadyDelivered || held),
                $"seed '{seed.Name}' from {peer}: the recorded valid datagram was neither delivered, " +
                $"nor deduped, nor stale, nor held behind an ordering hole (events {end.Events.Count}, " +
                $"held {heldBefore} -> {session.InboundOrdering.HeldRecordCount}, " +
                $"consumed {alreadyConsumed}, slot {orderedIndex} taken {slotAlreadyDelivered})");

            Assert.True(
                session.InboundOrdering.HeldRecordCount <=
                    InboundOrdering.MaxPendingMessagesPerChannel * InboundOrdering.MaxOrderingChannels,
                $"seed '{seed.Name}': {peer} held {session.InboundOrdering.HeldRecordCount} records, over the cap");
        }

        // A repeat of the same datagram is re-acked, never re-delivered, whichever way the first
        // injection went.
        end.Events.Clear();
        end.Wrapper.InjectDatagram(
            CaptureVectors.RpcPacketWire83Bytes, CaptureVectors.RpcPacketWire83Bytes.Length, peer);
        Assert.True(
            end.Events.Count == 0,
            $"seed '{seed.Name}' from {peer}: a repeated datagram was delivered {end.Events.Count} more time(s)");
    }

    /// <summary>
    /// Property 5: a peer that sent nothing must be unaffected by another peer's garbage, and an
    /// address the server has never seen must still be able to join. Peer C is never sent a mutation,
    /// so the strict form of the property applies to it unconditionally.
    /// </summary>
    private void AssertUntouchedPeersStillHaveSessions(FuzzEnd end, FuzzSeed seed)
    {
        foreach (IPEndPoint peer in new[] { PeerB, PeerC })
        {
            Assert.True(
                SessionOrNull(end, peer) is not null,
                $"seed '{seed.Name}': the attacker's garbage destroyed the session for {peer}");
        }
    }

    /// <summary>
    /// Called once per run, not once per seed: the same recorded datagram can only be delivered to a
    /// given peer once, so a per-seed repeat would measure the dedupe path rather than the property.
    /// </summary>
    private void AssertUntouchedPeersStillDeliver(FuzzEnd end, FuzzSeed seed)
    {
        AssertUntouchedPeersStillHaveSessions(end, seed);
        AssertSessionStillDelivers(end, seed, PeerB, allowDocumentedDeviation: false);
        AssertSessionStillDelivers(end, seed, PeerC, allowDocumentedDeviation: false);
    }

    /// <summary>The sequence numbers the recorded <c>wire=0x83</c> datagram carries.</summary>
    private static uint[] RpcWire83Sequences()
    {
        if (!RakNetDatagram.TryParse(
                CaptureVectors.RpcPacketWire83Bytes,
                CaptureVectors.RpcPacketWire83Bytes.Length,
                out RakNetDatagram parsed,
                out string error))
        {
            throw new InvalidOperationException($"the recorded wire=0x83 datagram no longer parses: {error}");
        }

        return parsed.Packets.Select(record => record.SequenceNumber).ToArray();
    }

    /// <summary>
    /// The ordering slot the recorded <c>wire=0x83</c> datagram claims, read from its own bytes rather
    /// than assumed. A channel whose expected index has moved past this slot will (correctly) drop the
    /// datagram as stale, which is a consequence of the peer's own garbage, not of a parser defect.
    /// </summary>
    private static (byte Channel, uint OrderedIndex) RpcWire83OrderingSlot()
    {
        if (!RakNetDatagram.TryParse(
                CaptureVectors.RpcPacketWire83Bytes,
                CaptureVectors.RpcPacketWire83Bytes.Length,
                out RakNetDatagram parsed,
                out string error))
        {
            throw new InvalidOperationException($"the recorded wire=0x83 datagram no longer parses: {error}");
        }

        foreach (RakNetPacketRecord record in parsed.Packets)
        {
            if (((WireReliability)record.Reliability).CarriesOrdering())
                return (record.OrderingChannel, record.OrderingIndex);
        }

        throw new InvalidOperationException("the recorded wire=0x83 datagram carries no ordered record");
    }

    private static MtaNetSession? SessionOrNull(FuzzEnd end, IPEndPoint peer) =>
        end.Wrapper.FindSession(MtaNetWrapper.BinaryAddressOf(peer));

    private static MtaNetSession Session(FuzzEnd end, IPEndPoint peer) =>
        SessionOrNull(end, peer) ?? throw new InvalidOperationException($"no session for {peer}");
}

/// <summary>
/// <see cref="FactAttribute"/> that skips unless <c>MTANET_FUZZ=1</c>, matching
/// <see cref="ProbeFactAttribute"/> and <see cref="SocketFactAttribute"/>: xunit 2.x has no dynamic
/// skip, so an opt-in run is a skip and not a no-op.
/// </summary>
internal sealed class FuzzFactAttribute : FactAttribute
{
    internal const string GateVariable = "MTANET_FUZZ";

    public FuzzFactAttribute()
    {
        if (Environment.GetEnvironmentVariable(GateVariable) != "1")
            this.Skip = $"Opt-in deep fuzz pass; set {GateVariable}=1 to run it.";
    }
}
