using System.Net;
using SlipeServer.Net.Managed;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Pins the whole admission predicate as a cross-product rather than as a set of examples:
/// <c>admit ⟺ gate2 ∧ (gate3 ∨ (gate4 ∧ whitelisted))</c>, evaluated over every reachable session
/// state and every whitelist class, and then tightened by the one receive-side version gate
/// (findings/transport-core.md §21.11).
/// </summary>
/// <remarks>
/// The predicate is computed twice — once by calling the public gate methods directly, once by
/// calling <see cref="MtaAdmission.Evaluate"/> — so the test fails if the composition drifts even
/// though every individual gate still behaves. Examples only cover the states someone thought of; a
/// cross-product covers the ones they did not.
/// </remarks>
public class MtaAdmissionTruthTableTests
{
    private const uint ClientIp = 0xC0A8B21Eu;

    private static IPEndPoint EndPoint => new(IPAddress.Parse("192.168.178.30"), 58057);

    /// <summary>
    /// The named session states, chosen so that gate 3 and gate 4 hit every reachable combination.
    /// Gate 4 is the exact complement of gate 3's post-handshake arm, so <c>(true, true)</c> is not
    /// reachable and is asserted as such below rather than tabulated.
    /// </summary>
    private static readonly string[] AllStates =
    {
        "not-established",
        "rejected",
        "flag854",
        "flag855",
        "pre-handshake",
        "post-handshake-early",
        "post-handshake-late-quiet",
        "post-handshake-late-loud",
        "flood-latched",
        "usage-exhausted",
    };

    public static TheoryData<string> States
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (string state in AllStates)
                data.Add(state);
            return data;
        }
    }

    private static MtaNetSession Session(string state)
    {
        var session = new MtaNetSession(((ulong)ClientIp << 32) + (uint)EndPoint.Port, EndPoint)
        {
            IsEstablished = true,
            HandshakeComplete = true,
            Counter97d = 0,
        };

        switch (state)
        {
            case "not-established":
                session.IsEstablished = false;
                break;
            case "rejected":
                session.Rejected = true;
                break;
            case "flag854":
                session.Flag854 = true;
                break;
            case "flag855":
                session.Flag855 = true;
                break;
            case "pre-handshake":
                session.HandshakeComplete = false;
                break;
            case "post-handshake-early":
                session.Counter97d = 0;
                break;
            case "post-handshake-late-quiet":
                // Counter past 2 with the "quiet" latch: gate 3 denies, gate 4 admits.
                session.Counter97d = 2;
                session.Flag97e = true;
                session.Flag97f = false;
                break;
            case "post-handshake-late-loud":
                // Counter past 2 with the "loud" latch: gate 3 admits and bypasses everything.
                session.Counter97d = 2;
                session.Flag97e = true;
                session.Flag97f = true;
                break;
            case "flood-latched":
                session.FloodNoticeLatched = true;
                break;
            case "usage-exhausted":
                session.InboundPacketCounts[32] = 5;
                break;
        }

        return session;
    }

    private static MtaAdmissionOptions OptionsFor(string state)
    {
        var options = new MtaAdmissionOptions();
        if (state == "usage-exhausted")
            options.Gate2UsageLimits[32] = 5;
        return options;
    }

    /// <summary>
    /// (wire id, expected 0-based mtaId, second payload byte). The payload byte only matters for
    /// mtaId 0x18, whose whitelist entry is conditional on it.
    /// </summary>
    private static readonly (byte Wire, byte Id, byte Second)[] Packets =
    {
        (0x66, 3, 0x00),   // PLAYER_JOIN — never whitelisted
        (0x76, 4, 0x00),   // PLAYER_JOINDATA — never whitelisted, and carries the version gate
        (0x83, 32, 0x00),  // PLAYER_PURESYNC — never whitelisted
        (0x85, 34, 0x00),  // 0x22 — always whitelisted
        (0xCF, 108, 0x00), // 0x6C — always whitelisted
        (0x7B, 24, 0x00),  // 0x18 with payload[1] = 0 — whitelisted
        (0x7B, 24, 0x05),  // 0x18 with payload[1] = 5 — not whitelisted
    };

    [Theory]
    [MemberData(nameof(States))]
    public void TheVerdictIsGateTwoAndGateThreeOrGateFourAndTheWhitelist(string state)
    {
        foreach ((byte wireId, byte expectedId, byte secondByte) in Packets)
        {
            foreach (ushort negotiated in new ushort[] { 0, 0x90 })
            {
                byte[] payload = { wireId, secondByte, 0x00, 0x00 };
                const int BitLength = 32;
                MtaAdmissionOptions options = OptionsFor(state);

                MtaNetSession probe = Session(state);
                probe.NegotiatedBitStreamVersion = negotiated;

                bool gate2 = MtaAdmission.Gate2(probe, expectedId, BitLength, options);
                bool gate3 = MtaAdmission.Gate3(probe, expectedId);
                bool gate4 = MtaAdmission.Gate4(probe);

                bool whitelisted = MtaProtocol.IsWhitelisted(expectedId, payload, BitLength);

                // The documented predicate, plus the single version gate, which sits after the
                // whitelist and applies only to mtaId 4.
                bool expected =
                    gate2
                    && (gate3 || (gate4 && whitelisted))
                    && !(expectedId == MtaGamePackets.PlayerJoinDataMtaId && negotiated != 0);

                MtaNetSession session = Session(state);
                session.NegotiatedBitStreamVersion = negotiated;

                MtaAdmissionResult actual = MtaAdmission.Evaluate(
                    session, wireId, payload, BitLength, options, out byte mtaId, out string reason);

                Assert.Equal(expectedId, mtaId);
                Assert.True(
                    expected == (actual == MtaAdmissionResult.Deliver),
                    $"{state} wire 0x{wireId:X2} id {expectedId} version 0x{negotiated:X4} " +
                    $"second 0x{secondByte:X2}: gate2={gate2} gate3={gate3} gate4={gate4} " +
                    $"whitelist={whitelisted} expected={expected} actual={actual} ({reason})");
            }
        }
    }

    /// <summary>
    /// Gate 4 is the exact complement of gate 3's post-handshake arm, which is what makes the
    /// "gate 3 bypasses gate 4 and the whitelist" shortcut safe: the two can never both admit, so a
    /// packet never gets two independent reasons to be delivered.
    /// </summary>
    [Theory]
    [MemberData(nameof(States))]
    public void GateThreeAndGateFourAreNeverBothTrue(string state)
    {
        foreach (byte id in new byte[] { 3, 4, 24, 32, 34, 85, 108 })
        {
            MtaNetSession session = Session(state);
            Assert.False(
                MtaAdmission.Gate3(session, id) && MtaAdmission.Gate4(session),
                $"{state} id {id}: gates 3 and 4 both admitted");
        }
    }

    /// <summary>
    /// A gate-3 pass skips the whitelist entirely. This is the only reason a join can complete: every
    /// packet in the join sequence carries an id the whitelist rejects.
    /// </summary>
    [Fact]
    public void AGateThreePassAdmitsEvenAnIdTheWhitelistRejects()
    {
        MtaNetSession session = Session("pre-handshake");
        Assert.False(MtaProtocol.IsWhitelisted(MtaGamePackets.PlayerJoinDataMtaId, new byte[] { 0x76, 0 }, 16));
        Assert.True(MtaAdmission.Gate3(session, MtaGamePackets.PlayerJoinDataMtaId));
        Assert.False(MtaAdmission.Gate4(session));

        Assert.Equal(
            MtaAdmissionResult.Deliver,
            MtaAdmission.Evaluate(
                session, 0x76, new byte[] { 0x76, 0 }, 16, new MtaAdmissionOptions(),
                out byte mtaId, out _));
        Assert.Equal(MtaGamePackets.PlayerJoinDataMtaId, mtaId);
    }

    /// <summary>
    /// The silent-drop invariant: every rejection is a <see cref="MtaAdmissionResult.Drop"/> with a
    /// reason string, and nothing else. There is no exception path, no disconnect, and no reply —
    /// the transport is expected to drop the packet and move on.
    /// </summary>
    [Fact]
    public void EveryRejectionIsADropWithAReasonAndNoException()
    {
        int drops = 0;

        foreach (string state in AllStates)
        {
            foreach ((byte wireId, byte id, byte secondByte) in Packets)
            {
                MtaNetSession session = Session(state);
                MtaAdmissionResult result = MtaAdmission.Evaluate(
                    session, wireId, new byte[] { wireId, secondByte, 0, 0 }, 32,
                    OptionsFor(state), out _, out string reason);

                if (result != MtaAdmissionResult.Drop)
                    continue;

                drops++;
                Assert.False(string.IsNullOrWhiteSpace(reason));
            }
        }

        Assert.True(drops > 0, "the table must contain at least one rejecting state");
    }

    /// <summary>
    /// Gate 2 is inclusive on both budgets: a packet that lands exactly on the budget is admitted and
    /// the next one latches. Getting the comparison off by one either drops a legitimate packet or
    /// lets one extra through, and both are invisible until a client is under load.
    /// </summary>
    [Fact]
    public void GateTwoBudgetsAreInclusive()
    {
        const byte Id = 32;

        var countOptions = new MtaAdmissionOptions();
        countOptions.Gate2PacketBudgets[Id] = 3;
        MtaNetSession counted = Session("post-handshake-early");
        for (int i = 0; i < 3; i++)
            Assert.True(MtaAdmission.Gate2(counted, Id, 8, countOptions));
        Assert.False(MtaAdmission.Gate2(counted, Id, 8, countOptions));
        Assert.True(counted.FloodNoticeLatched);

        var bitOptions = new MtaAdmissionOptions();
        bitOptions.Gate2BitBudgets[Id] = 24;
        MtaNetSession bits = Session("post-handshake-early");
        Assert.True(MtaAdmission.Gate2(bits, Id, 8, bitOptions));
        Assert.True(MtaAdmission.Gate2(bits, Id, 8, bitOptions));
        Assert.True(MtaAdmission.Gate2(bits, Id, 8, bitOptions));
        Assert.False(MtaAdmission.Gate2(bits, Id, 8, bitOptions));
        Assert.True(bits.FloodNoticeLatched);
    }

    /// <summary>
    /// The gate-2 accumulators are per id and never shared: exhausting one id must not touch another.
    /// A single flat counter would look correct in a one-id test and drop half the traffic in a real
    /// session.
    /// </summary>
    [Fact]
    public void GateTwoAccumulatorsArePerId()
    {
        var options = new MtaAdmissionOptions();
        options.Gate2PacketBudgets[32] = 1;

        MtaNetSession session = Session("post-handshake-early");
        Assert.True(MtaAdmission.Gate2(session, 32, 8, options));
        Assert.False(MtaAdmission.Gate2(session, 32, 8, options));

        Assert.True(session.InboundPacketCounts[32] > 0);
        Assert.Equal(0, session.InboundPacketCounts[85]);
        Assert.Equal(0, session.InboundBitTotals[85]);

        // The latch is global, so a second id is refused as well once it is set.
        Assert.False(MtaAdmission.Gate2(session, 85, 8, options));
    }
}
