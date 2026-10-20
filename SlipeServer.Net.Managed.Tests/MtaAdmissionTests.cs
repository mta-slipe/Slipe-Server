using System.Net;
using SlipeServer.Net.Managed;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Pins the receive-side admission policy from findings/transport-core.md §21.10–§21.11.
/// The load-bearing property is that the policy is a DISJUNCTION: a gate-3 pass delivers
/// immediately, skipping gate 4 and the whitelist.
/// </summary>
public class MtaAdmissionTests
{
    private static MtaNetSession NewSession(ulong binaryAddress = 0x0100007F00009C40ul) =>
        new(binaryAddress, new IPEndPoint(IPAddress.Loopback, 40000));

    /// <summary>A session in the gate-3 bypass window: established, handshake done, counter 0.</summary>
    private static MtaNetSession BypassSession()
    {
        var session = NewSession();
        session.HandshakeComplete = true;
        session.IsEstablished = true;
        return session;
    }

    /// <summary>A session where gate 3 fails but gate 4 passes, so the whitelist is reached.</summary>
    private static MtaNetSession Gate4Session()
    {
        var session = NewSession();
        session.HandshakeComplete = true;
        session.IsEstablished = true;
        session.Counter97d = 2;
        session.Flag97e = true;
        session.Flag97f = false;
        return session;
    }

    private static byte[] Payload(byte wireId, int extra = 8) =>
        new[] { wireId }.Concat(new byte[extra]).ToArray();

    [Fact]
    public void Gate3AndGate4_AreMutuallyExclusive()
    {
        var bypass = BypassSession();
        Assert.True(MtaAdmission.Gate3(bypass, 0x01));
        Assert.False(MtaAdmission.Gate4(bypass));

        var gate4 = Gate4Session();
        Assert.False(MtaAdmission.Gate3(gate4, 0x01));
        Assert.True(MtaAdmission.Gate4(gate4));
    }

    [Fact]
    public void Gate3Bypass_DeliversAnUnwhitelistedId()
    {
        // 0x01 is neither whitelisted nor the pre-handshake pair, and the conjunction reading
        // would drop it because gate 4 is false. The binary's JNZ at 0x100253ea delivers it.
        var session = BypassSession();
        var wireId = (byte)(0x01 + MtaProtocol.MtaIdToWireId);

        var verdict = MtaAdmission.Evaluate(
            session, wireId, Payload(wireId), 72, new MtaAdmissionOptions(), out var mtaId, out var reason);

        Assert.Equal(MtaAdmissionResult.Deliver, verdict);
        Assert.Equal(0x01, mtaId);
        Assert.Empty(reason);
    }

    [Fact]
    public void WhitelistedId_DeliversThroughGate4()
    {
        var session = Gate4Session();
        var wireId = (byte)(0x22 + MtaProtocol.MtaIdToWireId);

        Assert.Equal(
            MtaAdmissionResult.Deliver,
            MtaAdmission.Evaluate(session, wireId, Payload(wireId), 72, new MtaAdmissionOptions(), out var mtaId, out _));
        Assert.Equal(0x22, mtaId);
    }

    [Fact]
    public void NonWhitelistedId_DropsThroughGate4()
    {
        var session = Gate4Session();
        var wireId = (byte)(0x01 + MtaProtocol.MtaIdToWireId);

        Assert.Equal(
            MtaAdmissionResult.Drop,
            MtaAdmission.Evaluate(session, wireId, Payload(wireId), 72, new MtaAdmissionOptions(), out _, out var reason));
        Assert.Contains("whitelist", reason);
    }

    [Fact]
    public void MtaId4_IsDroppedOnceAVersionIsNegotiated()
    {
        // The only receive-side version gate: 0x10025488 CMP word [EDI+0x938],0 / 0x10025490 JNZ
        // 0x10025894, where 0x10025894 is the shared silent-drop epilogue (both branch targets read:
        // the accept arm calls FUN_1002a280 to raise the packet). mtaId 4 is not whitelisted, so it can
        // only reach this gate through the gate-3 bypass; and because Evaluate swaps 4 <-> 0x13 before
        // the gates, the wire id must be 0x13 + 99 to resolve to mtaId 4.
        var session = BypassSession();
        var wireId = (byte)(0x13 + MtaProtocol.MtaIdToWireId);

        session.NegotiatedBitStreamVersion = 0;
        Assert.Equal(
            MtaAdmissionResult.Deliver,
            MtaAdmission.Evaluate(session, wireId, Payload(wireId), 72, new MtaAdmissionOptions(), out var mtaId, out _));
        Assert.Equal(4, mtaId);

        session.NegotiatedBitStreamVersion = 0x90;
        Assert.Equal(
            MtaAdmissionResult.Drop,
            MtaAdmission.Evaluate(session, wireId, Payload(wireId), 72, new MtaAdmissionOptions(), out _, out var reason));
        Assert.Contains("version", reason);
    }

    [Fact]
    public void IdSwapHappensBeforeTheGates()
    {
        // Wire 0x13 + 99 resolves to mtaId 4 after the swap.
        var session = BypassSession();
        var wireId = (byte)(0x13 + MtaProtocol.MtaIdToWireId);

        Assert.Equal(
            MtaAdmissionResult.Deliver,
            MtaAdmission.Evaluate(session, wireId, Payload(wireId), 72, new MtaAdmissionOptions(), out var mtaId, out _));
        Assert.Equal(4, mtaId);
    }

    [Fact]
    public void TheOutboundWireIdRoundTripsThroughEvaluate()
    {
        // Locks the egress id and the ingress swap together: whatever ToWireId emits for an mtaId
        // must be admitted as that same mtaId. Without this, PLAYER_JOINDATA went out as 0x67 and
        // came back in as mtaId 0x13 (PACKET_ID_MTA_RESERVED_13). mtaId 0 is excluded because the
        // native path drops it outright (mtaId != 0 && mtaId != 0xFF, §5.1).
        foreach (byte mtaId in new byte[] { 2, 3, 4, 23, 0x13, 103 })
        {
            var session = BypassSession();
            byte wireId = MtaProtocol.ToWireId(mtaId);

            Assert.Equal(
                MtaAdmissionResult.Deliver,
                MtaAdmission.Evaluate(session, wireId, Payload(wireId), 72, new MtaAdmissionOptions(), out var admitted, out _));
            Assert.Equal(mtaId, admitted);
        }
    }

    [Fact]
    public void TheEightByteFloorIsADatagramTest_NotAPayloadTest()
    {
        // DoPulse row 1 tests the received datagram's byte length (pkt[+0x20] >= 8) before it looks at
        // any packet, so the floor is enforced by the datagram handler, not by admission.
        // RID_DISCONNECTION_NOTIFICATION is a legitimate 1-byte payload carried in a 20-byte datagram,
        // so admission must not apply the floor to the payload.
        Assert.Equal(8, MtaAdmission.MinimumDatagramBytes);

        var session = BypassSession();
        var wireId = (byte)(0x01 + MtaProtocol.MtaIdToWireId);

        Assert.Equal(
            MtaAdmissionResult.Deliver,
            MtaAdmission.Evaluate(session, wireId, new[] { wireId }, 8, new MtaAdmissionOptions(), out _, out _));
    }

    [Fact]
    public void WireId0xFF_IsDropped()
    {
        Assert.Equal(
            MtaAdmissionResult.Drop,
            MtaAdmission.Evaluate(BypassSession(), 0xFF, Payload(0xFF), 72, new MtaAdmissionOptions(), out _, out var reason));
        Assert.Contains("0xff", reason);
    }

    [Fact]
    public void WireIdsBelowTheMtaBand_AreDroppedAsRakNetProtocolIds()
    {
        // 0x62 (98) is one below RID_USER_PACKET_ENUM, so it can never be a game packet. The native
        // server hands sub-99 ids to the reliability layer before DoPulse applies its band test.
        Assert.Equal(
            MtaAdmissionResult.Drop,
            MtaAdmission.Evaluate(BypassSession(), 0x62, Payload(0x62), 72, new MtaAdmissionOptions(), out _, out var reason));
        Assert.Contains("RakNet protocol id 98", reason);
        Assert.Contains("not a game packet", reason);
        Assert.DoesNotContain("outside", reason);
    }

    [Fact]
    public void ConnectionRequest_NeverReachesTheAdmissionGatesAsAGamePacket()
    {
        // The exact id the first real-client run hit. It is a RakNet protocol message, so the
        // diagnostic must name it rather than report a malformed game packet.
        Assert.Equal(
            MtaAdmissionResult.Drop,
            MtaAdmission.Evaluate(
                BypassSession(), MtaProtocol.Rid.ConnectionRequest, Payload(MtaProtocol.Rid.ConnectionRequest), 72,
                new MtaAdmissionOptions(), out _, out var reason));
        Assert.Contains("RID_CONNECTION_REQUEST", reason);
    }

    [Fact]
    public void Gate2LatchesAndThenRejectsEverything()
    {
        var options = new MtaAdmissionOptions();
        options.Gate2UsageLimits[0x01] = 2;
        var session = BypassSession();
        var wireId = (byte)(0x01 + MtaProtocol.MtaIdToWireId);
        var payload = Payload(wireId);

        Assert.Equal(MtaAdmissionResult.Deliver,
            MtaAdmission.Evaluate(session, wireId, payload, 72, options, out _, out _));
        Assert.Equal(MtaAdmissionResult.Deliver,
            MtaAdmission.Evaluate(session, wireId, payload, 72, options, out _, out _));

        // Third packet exceeds the limit and latches the notice.
        Assert.Equal(MtaAdmissionResult.Drop,
            MtaAdmission.Evaluate(session, wireId, payload, 72, options, out _, out var reason));
        Assert.Contains("gate 2", reason);
        Assert.True(session.FloodNoticeLatched);

        // Once latched, even a different id is refused.
        var other = (byte)(0x22 + MtaProtocol.MtaIdToWireId);
        Assert.Equal(MtaAdmissionResult.Drop,
            MtaAdmission.Evaluate(session, other, Payload(other), 72, options, out _, out _));
    }

    [Fact]
    public void Gate2DefaultsToUnlimited()
    {
        var options = new MtaAdmissionOptions();
        var session = BypassSession();
        var wireId = (byte)(0x01 + MtaProtocol.MtaIdToWireId);
        var payload = Payload(wireId);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(MtaAdmissionResult.Deliver,
                MtaAdmission.Evaluate(session, wireId, payload, 72, options, out _, out _));
        }
        Assert.False(session.FloodNoticeLatched);
    }

    [Fact]
    public void Gate1OnlyTracksMtaId4And0x5A()
    {
        var options = new MtaAdmissionOptions { Gate1RateLimit = 1 };

        // mtaId 0x01 is not tracked, so it is always allowed.
        Assert.True(MtaAdmission.Gate1(0x0A0B0C0Du, 0x01, options));
        Assert.True(MtaAdmission.Gate1(0x0A0B0C0Du, 0x01, options));
        Assert.True(MtaAdmission.Gate1(0x0A0B0C0Du, 0x01, options));

        // mtaId 4 is tracked with a reject threshold of 2 * limit.
        const uint ip = 0x0D0C0B0Au;
        Assert.True(MtaAdmission.Gate1(ip, 4, options));
        Assert.True(MtaAdmission.Gate1(ip, 4, options));
        Assert.False(MtaAdmission.Gate1(ip, 4, options));
    }

    [Fact]
    public void Gate1IsDisabledWhenTheLimitIsZero()
    {
        var options = new MtaAdmissionOptions();
        const uint ip = 0x0E0D0C0Bu;
        for (int i = 0; i < 50; i++)
        {
            Assert.True(MtaAdmission.Gate1(ip, 4, options));
        }
    }

    [Fact]
    public void Gate3PreHandshakeAdmitsOnly3And4()
    {
        var session = NewSession();
        session.IsEstablished = true;
        session.HandshakeComplete = false;

        Assert.True(MtaAdmission.Gate3(session, 3));
        Assert.True(MtaAdmission.Gate3(session, 4));
        Assert.False(MtaAdmission.Gate3(session, 0x22));
    }

    [Fact]
    public void Gate3RefusesRejectedSessions()
    {
        var session = BypassSession();
        Assert.True(MtaAdmission.Gate3(session, 0x01));

        session.Rejected = true;
        Assert.False(MtaAdmission.Gate3(session, 0x01));
    }
}
