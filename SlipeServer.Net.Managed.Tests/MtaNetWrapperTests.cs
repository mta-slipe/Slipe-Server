using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using SlipeServer.Net.Managed;
using SlipeServer.Net.Wrappers;
using SlipeServer.Net.Wrappers.Enums;
using SlipeServer.Packets.Enums;
using Xunit;

namespace SlipeServer.Net.Managed.Tests;

/// <summary>
/// Pins the contract <see cref="MtaNetWrapper"/> owes Slipe Server — the eleven
/// <see cref="INetWrapper"/> members, the <c>PacketReceived</c> five-tuple, the binary-address layout,
/// the session-metadata surface and the lifecycle.
///
/// <para>
/// This is the externally-consumed surface, so it is the one place where reflection is the right tool:
/// a refactor that renames or re-signs a member must fail here rather than at Slipe's call site.
/// Everything else goes through the public API, and every datagram fed in is a literal from
/// <see cref="CaptureVectors"/> or <c>capture.log</c> — never the port's own writer.
/// </para>
///
/// <para>
/// Only the lifecycle group calls <see cref="MtaNetWrapper.Start"/>, and it binds port 0 so it can
/// never collide with a real server or another test.
/// <see cref="MtaNetWrapper.InjectDatagram"/> is the documented single entry point for one received
/// datagram and needs no socket, and <c>SendRaw</c> records to <see cref="PacketCapture"/> before
/// touching the (null) socket — so a never-started wrapper still produces its full outbound byte
/// stream.
/// </para>
/// </summary>
public class MtaNetWrapperTests
{
    // ------------------------------------------------------------ INetWrapper conformance

    /// <summary>
    /// The interface shape Slipe Server compiles against, restated as data. If the submodule is bumped
    /// and the contract grows a member or changes a signature, this table stops matching and the test
    /// fails before a behavioural change can hide behind the interface.
    /// </summary>
    public static TheoryData<string, string, string> InterfaceMembers => new()
    {
        // name, return type, parameter types
        {
            "GetClientSerialExtraAndVersion",
            "System.Tuple`3[System.String,System.String,System.String]",
            "System.UInt64"
        },
        { "GetPlayerIp", "System.Net.IPAddress", "System.UInt64" },
        { "SendPacket", "System.Void", "System.UInt64,System.UInt16,SlipeServer.Packets.Packet" },
        {
            "SendPacket",
            "System.Void",
            "System.UInt64,SlipeServer.Packets.Enums.PacketId,System.UInt16,System.Byte[],"
            + "SlipeServer.Packets.Enums.PacketPriority,SlipeServer.Packets.Enums.PacketReliability"
        },
        { "SetVersion", "System.Void", "System.UInt64,System.UInt16" },
        { "ResendModPackets", "System.Void", "System.UInt64" },
        { "ResendPlayerACInfo", "System.Void", "System.UInt64" },
        {
            "SetAntiCheatConfig",
            "System.Void",
            "System.Collections.Generic.IEnumerable`1[SlipeServer.Net.Wrappers.Enums.AntiCheat],"
            + "System.Boolean,SlipeServer.Net.Wrappers.Enums.AllowGta3ImgMods,"
            + "System.Collections.Generic.IEnumerable`1[SlipeServer.Net.Wrappers.Enums.SpecialDetection],"
            + "SlipeServer.Net.Wrappers.Enums.DataFile"
        },
        { "Start", "System.Void", "" },
        { "Stop", "System.Void", "" },
    };

    [Fact]
    public void MtaNetWrapperImplementsINetWrapper()
    {
        Assert.True(typeof(INetWrapper).IsAssignableFrom(typeof(MtaNetWrapper)));
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(MtaNetWrapper)));
    }

    [Theory]
    [MemberData(nameof(InterfaceMembers))]
    public void InterfaceDeclaresTheDocumentedMember(string name, string returnType, string parameters)
    {
        MethodInfo? method = typeof(INetWrapper)
            .GetMethods()
            .SingleOrDefault(candidate => candidate.Name == name
                && string.Join(",", candidate.GetParameters().Select(p => p.ParameterType.ToString())) == parameters);

        Assert.NotNull(method);
        Assert.Equal(returnType, method!.ReturnType.ToString());
    }

    [Fact]
    public void InterfaceDeclaresPacketReceivedWithTheDocumentedSignature()
    {
        EventInfo? received = typeof(INetWrapper).GetEvent("PacketReceived");

        Assert.NotNull(received);
        Assert.Equal(
            "System.Action`5[SlipeServer.Net.Wrappers.INetWrapper,System.UInt64,"
            + "SlipeServer.Packets.Enums.PacketId,System.Byte[],System.Nullable`1[System.UInt32]]",
            received!.EventHandlerType!.ToString());
    }

    [Fact]
    public void EveryInterfaceMemberIsAPublicInstanceMemberOnTheWrapper()
    {
        // Not merely "assignable": Slipe resolves these through the interface, but the POC and these
        // tests call them directly, so an explicit interface implementation would be invisible.
        foreach (MethodInfo method in typeof(INetWrapper).GetMethods())
        {
            MethodInfo? implementation = typeof(MtaNetWrapper).GetMethod(
                method.Name,
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: method.GetParameters().Select(p => p.ParameterType).ToArray(),
                modifiers: null);

            Assert.True(implementation is not null, $"{method.Name} is not a public instance member");
            Assert.Equal(method.ReturnType, implementation!.ReturnType);
        }

        Assert.NotNull(typeof(MtaNetWrapper).GetEvent("PacketReceived", BindingFlags.Public | BindingFlags.Instance));
    }

    // ------------------------------------------------------------ binary address layout

    /// <summary>
    /// <c>((uint64)ipDword &lt;&lt; 32) + port</c>, with the IPv4 dword assembled little-endian from the
    /// four address bytes — the native <c>remoteSystem</c> key.
    /// </summary>
    [Theory]
    // 192.0.2.10 -> 10<<24 | 2<<16 | 0<<8 | 192 = 0x0A0200C0; 58057 = 0xE2C9.
    [InlineData("192.0.2.10", 58057, 0x0A0200C00000E2C9UL)]
    // 127.0.0.1 -> 0x0100007F. Proves little-endian: big-endian assembly would give 0x7F000001.
    [InlineData("127.0.0.1", 1, 0x0100007F00000001UL)]
    [InlineData("0.0.0.0", 0, 0UL)]
    [InlineData("255.255.255.255", 65535, 0xFFFFFFFF0000FFFFUL)]
    [InlineData("10.0.0.1", 50666, 0x0100000A0000C5EAUL)]
    public void BinaryAddressOfPacksTheIpv4DwordAboveThePort(string address, int port, ulong expected)
    {
        Assert.Equal(expected, MtaNetWrapper.BinaryAddressOf(new IPEndPoint(IPAddress.Parse(address), port)));
    }

    [Fact]
    public void BinaryAddressOfReturnsZeroForNonIpv4()
    {
        Assert.Equal(0UL, MtaNetWrapper.BinaryAddressOf(new IPEndPoint(IPAddress.IPv6Loopback, 50666)));
    }

    // ------------------------------------------------------------ PacketReceived five-tuple

    /// <summary>
    /// The <c>SERVER_JOIN_COMPLETE</c> recording (capture.log seq 14) delivered through
    /// <see cref="MtaNetWrapper.InjectDatagram"/>. Asserting all five arguments pins the <c>-99</c> wire
    /// offset, the one-byte strip, the binary address and the nullable ping in one test.
    /// </summary>
    [Fact]
    public void PacketReceivedCarriesTheFiveTupleForServerJoinComplete()
    {
        using var harness = new WrapperHarness();
        harness.OpenSession();

        harness.InjectHex(CaptureVectors.ServerJoinCompleteStock);

        Received received = Assert.Single(harness.Received);
        Assert.Same(harness.Wrapper, received.Sender);
        Assert.Equal(harness.Address, received.Address);
        Assert.Equal(2, (int)received.Id);
        Assert.Equal(PacketId.PACKET_ID_SERVER_JOIN_COMPLETE, received.Id);
        // The wire byte is 0x65 = 101 and 101 - 99 = 2; the payload loses exactly that leading byte.
        Assert.StartsWith("65", CaptureVectors.ServerJoinCompletePayloadStock, StringComparison.Ordinal);
        Assert.Equal(
            WireTestKit.HexOf(CaptureVectors.ServerJoinCompletePayloadStockBytes[1..]),
            WireTestKit.HexOf(received.Data));
        Assert.Null(received.Ping);
    }

    /// <summary>
    /// The real client's <c>PLAYER_JOINDATA</c> (capture.log seq 12): wire <c>0x76</c> must surface as
    /// packet id 4. The <c>0x76</c> is the <c>4 ⇄ 0x13</c> swap applied to 4 and then the <c>+99</c>
    /// offset, so this single byte pins both halves of the id mapping. The same datagram also carries
    /// <c>wire=0x7C</c>, which must surface as packet id 25.
    /// </summary>
    [Fact]
    public void PacketReceivedCarriesTheFiveTupleForPlayerJoinData()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.Inject(WireTestKit.CaptureLogRecord(12).Bytes);

        Assert.Equal(2, harness.Received.Count);
        Received received = harness.Received[0];
        Assert.Same(harness.Wrapper, received.Sender);
        Assert.Equal(harness.Address, received.Address);
        Assert.Equal(4, (int)received.Id);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_JOINDATA, received.Id);
        Assert.Null(received.Ping);
        Assert.NotEmpty(received.Data);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_LIST, harness.Received[1].Id);
    }

    /// <summary>
    /// A RakNet system packet (wire <c>0x00</c>) rides in the same datagram as an admitted
    /// <c>0x11</c>. Only the game packet may surface, and the id it surfaces with is the MTA one.
    /// </summary>
    [Fact]
    public void RakNetSystemPacketsInAMixedDatagramDoNotRaisePacketReceived()
    {
        using var harness = new WrapperHarness();
        harness.OpenSession();

        harness.InjectHex(CaptureVectors.IncomingConnectionAndSystemPacket);

        Received received = Assert.Single(harness.Received);
        Assert.Equal(3, (int)received.Id);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_JOIN, received.Id);
    }

    /// <summary>
    /// The capture.log steady-state RPC (<c>wire=0x83</c>) is packet id 32, and its payload is the
    /// datagram's packet payload minus the wire byte.
    /// </summary>
    [Fact]
    public void PacketReceivedCarriesTheStrippedPayloadForAnAdmittedRpc()
    {
        using var harness = new WrapperHarness();
        harness.OpenSession();

        harness.InjectHex(CaptureVectors.RpcPacketWire83);

        Received received = Assert.Single(harness.Received);
        Assert.Equal(32, (int)received.Id);
        Assert.Equal(PacketId.PACKET_ID_PLAYER_PURESYNC, received.Id);
        // The capture's own classification says bits=224, so the stripped view is 27 bytes: 28 payload
        // bytes minus the wire id.
        Assert.Equal(27, received.Data.Length);
        Assert.Equal(
            "0000000000180000000000000000000001006400008807D13B0000",
            WireTestKit.HexOf(received.Data));
    }

    [Fact]
    public void TwoPacketsInOneDatagramRaiseTwoEvents()
    {
        using var harness = new WrapperHarness();
        harness.OpenSession();

        harness.InjectHex(CaptureVectors.TwoWire7BPackets);

        Assert.Equal(2, harness.Received.Count);
        Assert.All(harness.Received, received => Assert.Equal(24, (int)received.Id));
        // bits=16 -> a one-byte stripped view; bits=24 -> a two-byte stripped view.
        Assert.Equal("00", WireTestKit.HexOf(harness.Received[0].Data));
        Assert.Equal("0300", WireTestKit.HexOf(harness.Received[1].Data));
    }

    [Fact]
    public void PacketReceivedDoesNotFireBeforeASessionExists()
    {
        using var harness = new WrapperHarness();

        // A connected-path datagram with no session record: HandleConnected drops it.
        harness.InjectHex(CaptureVectors.RpcPacketWire83);

        Assert.Empty(harness.Received);
    }

    // ------------------------------------------------------------ session metadata

    [Fact]
    public void GetClientSerialExtraAndVersionReturnsEmptyForAnUnknownPeer()
    {
        using var harness = new WrapperHarness();

        Tuple<string, string, string> identity = harness.Wrapper.GetClientSerialExtraAndVersion(0xDEADBEEF);

        Assert.Equal(string.Empty, identity.Item1);
        Assert.Equal(string.Empty, identity.Item2);
        Assert.Equal(string.Empty, identity.Item3);
    }

    [Fact]
    public void GetClientSerialExtraAndVersionReturnsEmptyBeforeTheJoinPacket()
    {
        using var harness = new WrapperHarness();
        harness.OpenSession();

        Tuple<string, string, string> identity = harness.Wrapper.GetClientSerialExtraAndVersion(harness.Address);

        Assert.Equal(string.Empty, identity.Item1);
        Assert.Equal(string.Empty, identity.Item2);
        Assert.Equal(string.Empty, identity.Item3);
    }

    /// <summary>
    /// The join packet is the only source of the serial and version Slipe reads back synchronously from
    /// <c>JoinDataPacketHandler</c>. The serial travels XOR-obfuscated ahead of the mod-level body, so
    /// this also proves the wrapper reads it before stripping the prefix.
    /// </summary>
    [Fact]
    public void GetClientSerialExtraAndVersionIsPopulatedByTheJoinPacket()
    {
        using var harness = new WrapperHarness();
        harness.ConnectSession();

        harness.Inject(WireTestKit.CaptureLogRecord(12).Bytes);

        Tuple<string, string, string> identity = harness.Wrapper.GetClientSerialExtraAndVersion(harness.Address);

        Assert.Equal("5DDE026C7AD482CF6D73B7B3B460FD5E", identity.Item1);
        Assert.Equal(string.Empty, identity.Item2);
        Assert.Equal("1.6.0-9.24149.0", identity.Item3);
    }

    [Fact]
    public void GetPlayerIpReturnsTheSessionAddress()
    {
        using var harness = new WrapperHarness();
        harness.OpenSession();

        Assert.Equal(IPAddress.Parse("192.0.2.10"), harness.Wrapper.GetPlayerIp(harness.Address));
    }

    [Fact]
    public void GetPlayerIpReturnsNoneForAnUnknownPeer()
    {
        using var harness = new WrapperHarness();

        Assert.Equal(IPAddress.None, harness.Wrapper.GetPlayerIp(0xDEADBEEF));
    }

    /// <summary>
    /// <c>SetVersion</c> records the negotiated bitstream version on a live session; that version is
    /// what gates the packet-2 rows at 0x60 and 0x66. An unknown peer is ignored rather than creating a
    /// record.
    /// </summary>
    [Fact]
    public void SetVersionIgnoresAnUnknownPeer()
    {
        using var harness = new WrapperHarness();
        harness.OpenSession();
        int before = harness.Wrapper.SessionCount;

        harness.Wrapper.SetVersion(0xDEADBEEF, 0x91);

        Assert.Equal(before, harness.Wrapper.SessionCount);
        Assert.Equal(1, harness.Wrapper.SessionCount);
    }

    /// <summary>
    /// The negotiated version changes what <c>SendPacket</c> emits for packet 2: below 0x60 the leading
    /// block collapses to the single bitmap dword, and the serial row at 0x66 disappears as well.
    /// </summary>
    [Fact]
    public void SetVersionChangesTheEmittedServerJoinComplete()
    {
        static string Send(ushort version)
        {
            using var harness = new WrapperHarness();
            harness.OpenSession();
            harness.Wrapper.SetVersion(harness.Address, version);
            harness.Wrapper.SendPacket(
                harness.Address,
                PacketId.PACKET_ID_SERVER_JOIN_COMPLETE,
                version,
                CaptureVectors.ServerJoinCompleteBodyBytes);
            // The open-connection reply from OpenSession is also a TX record; the join-complete is last.
            return WireTestKit.HexOf(harness.Tx.Last().Bytes);
        }

        string legacy = Send(0x5F);
        string modern = Send(0x91);

        Assert.NotEqual(legacy, modern);
        Assert.True(
            legacy.Length < modern.Length,
            $"a pre-0x60 leading block must be shorter: {legacy.Length} vs {modern.Length}");
    }

    // ------------------------------------------------------------ anti-cheat configuration

    [Fact]
    public void SetAntiCheatConfigRetainsEveryValue()
    {
        using var harness = new WrapperHarness();

        harness.Wrapper.SetAntiCheatConfig(
            new[] { AntiCheat.HealthArmourHackAC1, AntiCheat.TrainerAC4 },
            hideAntiCheatFromClient: true,
            AllowGta3ImgMods.Peds,
            new[] { SpecialDetection.DisallowWine },
            DataFile.handlingCfg | DataFile.weaponDat);

        Assert.Equal(
            new[] { AntiCheat.HealthArmourHackAC1, AntiCheat.TrainerAC4 },
            harness.Wrapper.DisabledAntiCheats);
        Assert.True(harness.Wrapper.HideAntiCheatFromClient);
        Assert.Equal(AllowGta3ImgMods.Peds, harness.Wrapper.AllowGta3ImgMods);
        Assert.Equal(new[] { SpecialDetection.DisallowWine }, harness.Wrapper.EnabledSpecialDetections);
        Assert.Equal(DataFile.handlingCfg | DataFile.weaponDat, harness.Wrapper.DisallowedDataFiles);
    }

    [Fact]
    public void SetAntiCheatConfigDefaultsToNothingDisabled()
    {
        using var harness = new WrapperHarness();

        Assert.Empty(harness.Wrapper.DisabledAntiCheats);
        Assert.False(harness.Wrapper.HideAntiCheatFromClient);
        Assert.Equal(AllowGta3ImgMods.None, harness.Wrapper.AllowGta3ImgMods);
        Assert.Empty(harness.Wrapper.EnabledSpecialDetections);
        Assert.Equal(DataFile.None, harness.Wrapper.DisallowedDataFiles);
    }

    [Fact]
    public void SetAntiCheatConfigAcceptsNullSequences()
    {
        using var harness = new WrapperHarness();

        harness.Wrapper.SetAntiCheatConfig(null!, false, AllowGta3ImgMods.None, null!, DataFile.None);

        Assert.Empty(harness.Wrapper.DisabledAntiCheats);
        Assert.Empty(harness.Wrapper.EnabledSpecialDetections);
    }

    /// <summary>
    /// Both resend helpers are trace-only in this port: they emit nothing on the wire. Pinned so a
    /// future implementation has to change this test deliberately.
    /// </summary>
    [Fact]
    public void ResendHelpersAreTraceOnly()
    {
        using var harness = new WrapperHarness();

        harness.Wrapper.ResendModPackets(0x0A0200C00000E2C9UL);
        harness.Wrapper.ResendPlayerACInfo(0x0A0200C00000E2C9UL);

        Assert.Contains(harness.Trace, line => line.StartsWith("ResendModPackets", StringComparison.Ordinal));
        Assert.Contains(harness.Trace, line => line.StartsWith("ResendPlayerACInfo", StringComparison.Ordinal));
        Assert.Empty(harness.Tx);
    }

    // ------------------------------------------------------------ lifecycle

    [Fact]
    public void StartTwiceThrowsTheDocumentedMessage()
    {
        using var wrapper = new MtaNetWrapper(port: 0);

        wrapper.Start();
        try
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(wrapper.Start);
            Assert.Equal("Net server already started.", exception.Message);
        }
        finally
        {
            wrapper.Stop();
        }
    }

    [Fact]
    public void StopWithoutStartThrowsTheDocumentedMessage()
    {
        using var wrapper = new MtaNetWrapper(port: 0);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(wrapper.Stop);
        Assert.Equal("Net server is not running.", exception.Message);
    }

    [Fact]
    public void StartTracesTheListeningAddressAndStopTracesStopped()
    {
        using var wrapper = new MtaNetWrapper(port: 0);
        var lines = new List<string>();
        wrapper.Trace = lines.Add;

        wrapper.Start();
        try
        {
            Assert.Contains(lines, line => line.StartsWith("listening on udp 0.0.0.0:", StringComparison.Ordinal));
        }
        finally
        {
            wrapper.Stop();
        }

        Assert.Contains("stopped", lines);
    }

    /// <summary>
    /// <c>Stop()</c> clears the session table, so every metadata lookup reverts to its empty answer.
    /// </summary>
    [Fact]
    public void StopClearsSessions()
    {
        using var wrapper = new MtaNetWrapper(port: 0);
        wrapper.Start();

        ulong address = MtaNetWrapper.BinaryAddressOf(WrapperHarness.CaptureLogPeer);
        wrapper.InjectDatagram(
            CaptureVectors.OpenRequestBytes,
            CaptureVectors.OpenRequestBytes.Length,
            WrapperHarness.CaptureLogPeer);

        Assert.Equal(1, wrapper.SessionCount);
        Assert.Equal(IPAddress.Parse("192.0.2.10"), wrapper.GetPlayerIp(address));

        wrapper.Stop();

        Assert.Equal(0, wrapper.SessionCount);
        Assert.Equal(IPAddress.None, wrapper.GetPlayerIp(address));
        Assert.Equal(string.Empty, wrapper.GetClientSerialExtraAndVersion(address).Item1);
    }

    [Fact]
    public void DisposeIsIdempotentAndStopsARunningServer()
    {
        var wrapper = new MtaNetWrapper(port: 0);
        wrapper.Start();

        wrapper.Dispose();
        wrapper.Dispose();

        Assert.Throws<InvalidOperationException>(wrapper.Stop);
    }

    [Fact]
    public void DisposeOnANeverStartedWrapperDoesNotThrow()
    {
        new MtaNetWrapper(port: 0).Dispose();
    }

    // ------------------------------------------------------------ port 0 advertises the bound port

    /// <summary>
    /// <c>Start()</c> advertises the socket's actual bound port, not the configured <c>port</c> field.
    /// Binding port 0 must therefore produce a real, non-zero port in the connected-handshake reply's
    /// first address slot.
    ///
    /// The assertion is made on the wire: the slot carries the complemented local address dword and the
    /// advertised port, so the old bug was visible as <c>80FFFFFE0000</c>.
    /// </summary>
    [Fact]
    public void StartOnPortZeroAdvertisesTheBoundPortInTheConnectedReply()
    {
        using var harness = new WrapperHarness(port: 0, host: "127.0.0.1");
        harness.Wrapper.Start();
        try
        {
            ushort boundPort = ListeningPort(harness.Trace);
            Assert.NotEqual(0, boundPort);

            harness.OpenSession();
            harness.InjectHex(CaptureVectors.ConnectedConnectionRequest);

            CaptureLine reply = harness.Tx.Last();
            // body = [0x0E][client address 6][systemIndex 2][slot0 6]...
            byte[] body = reply.Bytes[^ConnectedHandshake.ReplyLength..];
            Assert.Equal((byte)MtaProtocol.Rid.ConnectionRequestAccepted, body[0]);
            Assert.Equal(
                $"80FFFFFE{boundPort & 0xFF:X2}{boundPort >> 8:X2}",
                WireTestKit.HexOf(body[9..15]));
        }
        finally
        {
            harness.Wrapper.Stop();
        }
    }

    private static ushort ListeningPort(IEnumerable<string> trace)
    {
        string line = trace.Last(entry => entry.StartsWith("listening on udp ", StringComparison.Ordinal));
        return ushort.Parse(line[(line.LastIndexOf(':') + 1)..]);
    }

    /// <summary>
    /// With a wildcard host there is no local address to advertise, so the first address slot is the
    /// zero sentinel <c>FFFFFFFF0000</c> = <c>255.255.255.255:0</c>. This is the same shape the stock
    /// server emitted for slots 1-9 in capture.log seq 4.
    /// </summary>
    [Fact]
    public void WildcardHostAdvertisesTheZeroAddressSlot()
    {
        using var harness = new WrapperHarness(port: 0, host: "0.0.0.0");
        harness.Wrapper.Start();
        try
        {
            harness.OpenSession();
            harness.InjectHex(CaptureVectors.ConnectedConnectionRequest);

            CaptureLine reply = harness.Tx.Last();
            byte[] body = reply.Bytes[^ConnectedHandshake.ReplyLength..];
            Assert.Equal("FFFFFFFF0000", WireTestKit.HexOf(body[9..15]));
        }
        finally
        {
            harness.Wrapper.Stop();
        }
    }
}
