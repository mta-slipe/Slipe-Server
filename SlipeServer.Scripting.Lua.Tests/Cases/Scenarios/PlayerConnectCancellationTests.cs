using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using SlipeServer.DropInReplacement.PacketHandlers;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Enums;
using SlipeServer.Scripting.Lua.Tests.Tools;
using SlipeServer.Server;
using SlipeServer.Server.Bans;
using SlipeServer.Server.Tests.Tools;
using System.Net;

namespace SlipeServer.Scripting.Lua.Tests.Cases.Scenarios;

/// <summary>
/// Verifies that calling cancelEvent() inside an onPlayerConnect handler sends
/// PACKET_ID_SERVER_DISCONNECTED instead of PACKET_ID_SERVER_JOIN_COMPLETE,
/// and that not cancelling allows the join to complete normally.
/// </summary>
public class PlayerConnectCancellationTests
{
    private static ScriptingJoinDataPacketHandler CreateHandler(IMtaServer sut, IBanRepository banRepository) =>
        new(sut, banRepository, sut.GetRequiredService<IScriptEventRuntime>());

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnPlayerConnect_OnRoot_PreventsJoin(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        TestPacketContext context,
        IMtaServer sut)
    {
        Ban? outBan = null;
        banRepositoryMock
            .Setup(x => x.IsIpOrSerialBanned(It.IsAny<string?>(), It.IsAny<IPAddress?>(), out outBan))
            .Returns(false);

        sut.RunLuaScript("""
            addEventHandler("onPlayerConnect", root, function(name, ip, serial, version)
                assertPrint("event fired")
                cancelEvent()
            end)
            """);

        var handler = CreateHandler(sut, banRepositoryMock.Object);
        handler.HandlePacket(player.Client, new PlayerJoinDataPacket());

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        context.GetPacketCount(PacketId.PACKET_ID_SERVER_DISCONNECTED, player).Should().Be(1);
        context.GetPacketCount(PacketId.PACKET_ID_SERVER_JOIN_COMPLETE, player).Should().Be(0);
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void NoCancelEvent_InOnPlayerConnect_AllowsJoin(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        TestPacketContext context,
        IMtaServer sut)
    {
        Ban? outBan = null;
        banRepositoryMock
            .Setup(x => x.IsIpOrSerialBanned(It.IsAny<string?>(), It.IsAny<IPAddress?>(), out outBan))
            .Returns(false);

        sut.RunLuaScript("""
            addEventHandler("onPlayerConnect", root, function(name, ip, serial, version)
                assertPrint("event fired")
            end)
            """);

        var handler = CreateHandler(sut, banRepositoryMock.Object);
        handler.HandlePacket(player.Client, new PlayerJoinDataPacket());

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        context.GetPacketCount(PacketId.PACKET_ID_SERVER_JOIN_COMPLETE, player).Should().Be(1);
        context.GetPacketCount(PacketId.PACKET_ID_SERVER_DISCONNECTED, player).Should().Be(0);
    }

    [Theory]
    [ScriptingAutoDomainData(false)]
    public void CancelEvent_InOnPlayerConnect_WithReason_PreventsJoin(
        [Frozen] Mock<IBanRepository> banRepositoryMock,
        AssertDataProvider assertDataProvider,
        LightTestPlayer player,
        TestPacketContext context,
        IMtaServer sut)
    {
        Ban? outBan = null;
        banRepositoryMock
            .Setup(x => x.IsIpOrSerialBanned(It.IsAny<string?>(), It.IsAny<IPAddress?>(), out outBan))
            .Returns(false);

        sut.RunLuaScript("""
            addEventHandler("onPlayerConnect", root, function(name, ip, serial, version)
                assertPrint("event fired")
                cancelEvent(true, "banned from server")
            end)
            """);

        var handler = CreateHandler(sut, banRepositoryMock.Object);
        handler.HandlePacket(player.Client, new PlayerJoinDataPacket());

        assertDataProvider.AssertPrints.Should().ContainSingle().Which.Should().Be("event fired");
        context.GetPacketCount(PacketId.PACKET_ID_SERVER_DISCONNECTED, player).Should().Be(1);
        context.GetPacketCount(PacketId.PACKET_ID_SERVER_JOIN_COMPLETE, player).Should().Be(0);
    }
}
