using SlipeServer.Net.Managed;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// <c>PLAYER_QUIT</c> (mtaId 5, wire <c>0x68</c>) carries the same 16-bit registry value the
/// <c>PLAYER_LIST</c> anticheat block carries, on a gate of its own and with nothing else in front of
/// the game payload.
/// <para>
/// <c>CNetServer::SendPacket</c> (<c>FUN_100351c0</c>) handles mtaId <c>5</c> in its own arm: it
/// reads no per-player version, writes no anticheat strings, and tests only the packet view's
/// version — <c>if (0x49 &lt; view-&gt;Version())</c>, so <c>0x4a</c> and up. Above the gate it writes
/// one <c>Write&lt;unsigned short&gt;(FUN_1000d3c0())</c> before the mod-level body. The port wrote
/// nothing at all, so a 1.6.0 client at <c>0x91</c> started reading the game payload two bytes early.
/// </para>
/// <para>
/// The gate is deliberately <b>not</b> the roster's <c>0x3a</c>: the two arms test different
/// constants, and one <c>0x4a</c> would make the prefix appear on clients that expect it on neither,
/// or vanish on clients that expect it on both.
/// </para>
/// <para>
/// <b>Provenance of the body literal.</b> <c>0B0000</c> is the three-byte <c>PLAYER_QUIT</c> body the
/// port put on the wire for a two-client session — <c>captures/capture-20261019-112948.log</c> record
/// 452, <c>TX 192.0.2.80:49189 … 14000000180D8000001100680B0000</c>, logged as
/// <c>p0 wire=0x68 … bits=32 bytes=4</c>. The logged <c>bytes=</c> counts the leading wire id, so the
/// body is <c>0B 00 00</c>: an element id and a quit reason, exactly what the client's
/// <c>Packet_PlayerQuit</c> reads once the net layer has taken its prefix back off. That record is
/// the defect — four bytes where native writes six.
/// </para>
/// </summary>
public class PlayerQuitFramingTests
{
    /// <summary>
    /// The body of a <c>PLAYER_QUIT</c> as the port emitted it, before any net-layer prefix: element
    /// id <c>0x000b</c> and a zero quit reason.
    /// </summary>
    private const string QuitBodyHex = "0B0000";

    [Theory]
    [InlineData(0x4a)]
    [InlineData(0x91)]
    public void TheRegistryValuePrecedesTheQuitBodyAboveTheGate(ushort version)
    {
        byte[] framed = MtaGamePackets.PrependPlayerQuitRegistryValue(
            WireTestKit.Hex(QuitBodyHex),
            version,
            registryValue: 2);

        Assert.Equal("0200" + QuitBodyHex, WireTestKit.HexOf(framed));
    }

    /// <summary>
    /// Below <c>0x4a</c> the native arm writes nothing, so neither may the port. <c>0x49</c> is the
    /// last closed version — the assembly renders the test as a strict <c>0x49 &lt;</c>, which is why
    /// <c>0x4a</c> is the first open one.
    /// </summary>
    [Theory]
    [InlineData(0x00)]
    [InlineData(0x39)]
    [InlineData(0x49)]
    public void TheQuitBodyIsUntouchedBelowTheGate(ushort version)
    {
        byte[] framed = MtaGamePackets.PrependPlayerQuitRegistryValue(
            WireTestKit.Hex(QuitBodyHex),
            version,
            registryValue: 2);

        Assert.Equal(QuitBodyHex, WireTestKit.HexOf(framed));
    }

    /// <summary>
    /// The prefix is 16-bit little-endian and byte-aligned — the same encoding as the roster's
    /// registry value, which is why one helper's bytes can be read by the other's test.
    /// </summary>
    [Theory]
    [InlineData(0, "0000")]
    [InlineData(2, "0200")]
    [InlineData(0x0102, "0201")]
    [InlineData(0xffff, "ffff")]
    public void TheRegistryValueIsWrittenLowByteFirst(ushort registryValue, string expectedPrefix)
    {
        byte[] framed = MtaGamePackets.PrependPlayerQuitRegistryValue(
            WireTestKit.Hex(QuitBodyHex),
            0x91,
            registryValue);

        Assert.Equal(
            (expectedPrefix + QuitBodyHex).ToUpperInvariant(),
            WireTestKit.HexOf(framed));
    }

    /// <summary>
    /// End to end through <c>SendPacket</c>: the wire id, then the registry value for the session
    /// count at send time, then the game payload. Two sessions is the smallest count that is not
    /// indistinguishable from an empty list, so it pins both the source of the value and its
    /// position.
    /// </summary>
    [Fact]
    public void TheWrapperEmitsTheRegistryPlayerCountAheadOfTheQuitBody()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();
        harness.ConnectSession(WireTestKit.Endpoint("192.0.2.80:49189"));

        harness.Wrapper.SendPacket(
            harness.Address,
            PacketId.PACKET_ID_PLAYER_QUIT,
            0x91,
            WireTestKit.Hex(QuitBodyHex));

        byte wireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_QUIT);
        string want = WireTestKit.HexOf([wireId, 0x02, 0x00, .. WireTestKit.Hex(QuitBodyHex)]);

        string tx = WireTestKit.HexOf(harness.Tx[^1].Bytes);
        Assert.True(tx.EndsWith(want, StringComparison.Ordinal), $"tx ({tx.Length / 2}B) = {tx}");
        Assert.Equal(6, want.Length / 2);

        Assert.Contains("registry value 2", harness.Trace[^1], StringComparison.Ordinal);
    }

    /// <summary>
    /// A pre-1.6.0 client must still get the bare body. The wrapper's gate comes from the same
    /// version it is handed, not from the registry, so the two arms stay independent.
    /// </summary>
    [Fact]
    public void TheWrapperLeavesTheQuitBodyAloneBelowTheGate()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.Wrapper.SendPacket(
            harness.Address,
            PacketId.PACKET_ID_PLAYER_QUIT,
            0x49,
            WireTestKit.Hex(QuitBodyHex));

        byte wireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_QUIT);
        string want = WireTestKit.HexOf([wireId, .. WireTestKit.Hex(QuitBodyHex)]);

        string tx = WireTestKit.HexOf(harness.Tx[^1].Bytes);
        Assert.True(tx.EndsWith(want, StringComparison.Ordinal), $"tx ({tx.Length / 2}B) = {tx}");
    }

    /// <summary>
    /// Both packets take the value from one source, so a roster and a quit sent in the same session
    /// agree. Native reads it through the same accessor for both.
    /// </summary>
    [Fact]
    public void TheRosterAndTheQuitCarryTheSameRegistryValue()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();
        harness.ConnectSession(WireTestKit.Endpoint("192.0.2.80:49189"));

        harness.Wrapper.SendPacket(
            harness.Address,
            PacketId.PACKET_ID_PLAYER_LIST,
            0x91,
            [0x80]);

        byte rosterWireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_LIST);
        string rosterWant = WireTestKit.HexOf([rosterWireId, 0x02, 0x00, 0x01, 0x00, 0x30, 0x00, 0x00, 0x80]);

        string roster = WireTestKit.HexOf(harness.Tx[^1].Bytes);
        Assert.True(roster.EndsWith(rosterWant, StringComparison.Ordinal), $"tx ({roster.Length / 2}B) = {roster}");

        harness.Wrapper.SendPacket(
            harness.Address,
            PacketId.PACKET_ID_PLAYER_QUIT,
            0x91,
            WireTestKit.Hex(QuitBodyHex));

        byte quitWireId = MtaProtocol.ToWireId((byte)(int)PacketId.PACKET_ID_PLAYER_QUIT);
        string quitWant = WireTestKit.HexOf([quitWireId, 0x02, 0x00, .. WireTestKit.Hex(QuitBodyHex)]);

        string quit = WireTestKit.HexOf(harness.Tx[^1].Bytes);
        Assert.True(quit.EndsWith(quitWant, StringComparison.Ordinal), $"tx ({quit.Length / 2}B) = {quit}");
    }
}
