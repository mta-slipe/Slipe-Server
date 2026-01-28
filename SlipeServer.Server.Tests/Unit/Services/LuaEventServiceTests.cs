using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class LuaEventServiceTests
{
    [Theory]
    [AutoDomainData]
    public void TriggerEvent_ShouldBroadcastToAllPlayers(
        LuaEventService sut,
        TestPacketContext context,
        LightTestMtaServer server
    )
    {
        // Arrange
        var players = new[] {
            server.CreatePlayer(),
            server.CreatePlayer(),
            server.CreatePlayer()
        };

        // Act
        sut.TriggerEvent("testEvent");

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_EVENT, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void TriggerEvent_WithLuaValues_ShouldBroadcastWithParameters(
        LuaEventService sut,
        TestPacketContext context,
        LightTestMtaServer server
    )
    {
        // Arrange
        var players = new[] {
            server.CreatePlayer(),
            server.CreatePlayer()
        };
        var parameters = new LuaValue[] {
            new LuaValue(42),
            new LuaValue("test")
        };

        // Act
        sut.TriggerEvent("testEvent", null, parameters);

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_EVENT, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void TriggerEventFor_SinglePlayer_ShouldSendPacketToPlayer(
        LuaEventService sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.TriggerEventFor(player, "testEvent");

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_EVENT, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void TriggerEventFor_WithParameters_ShouldSendPacketWithData(
        LuaEventService sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Arrange
        var parameters = new LuaValue[] { new LuaValue(123), new LuaValue("data") };

        // Act
        sut.TriggerEventFor(player, "testEvent", null, parameters);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_EVENT, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void TriggerEventFor_WithObjectParameters_ShouldMapAndSend(
        LuaEventService sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.TriggerEventFor(player, "testEvent", null, 42, "test", true);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_EVENT, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void TriggerEventForMany_MultiplePlayers_ShouldSendPacketToAllPlayers(
        LuaEventService sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        // Act
        sut.TriggerEventForMany(players, "testEvent");

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_EVENT, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void TriggerEventForMany_WithParameters_ShouldSendPacketWithData(
        LuaEventService sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        // Arrange
        var parameters = new LuaValue[] { new LuaValue(456) };

        // Act
        sut.TriggerEventForMany(players, "testEvent", null, parameters);

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_EVENT, player, count: 1);
    }
}
