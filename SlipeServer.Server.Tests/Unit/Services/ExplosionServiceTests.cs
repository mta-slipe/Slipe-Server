using FluentAssertions;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using System.Linq;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class ExplosionServiceTests
{
    [Theory]
    [AutoDomainData]
    public void CreateExplosion_ShouldBroadcastToAllPlayers(
        ExplosionService sut,
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
        var position = new Vector3(100, 200, 10);
        var type = ExplosionType.Grenade;

        // Act
        sut.CreateExplosion(position, type);

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_EXPLOSION, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateExplosionFor_SinglePlayer_ShouldSendPacketToPlayer(
        ExplosionService sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Arrange
        var position = new Vector3(100, 200, 10);
        var type = ExplosionType.Car;

        // Act
        sut.CreateExplosionFor([player], position, type);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_EXPLOSION, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateExplosionFor_MultiplePlayers_ShouldSendPacketToAllPlayers(
        ExplosionService sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        // Arrange
        var position = new Vector3(100, 200, 10);
        var type = ExplosionType.Rocket;

        // Act
        sut.CreateExplosionFor(players, position, type);

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_EXPLOSION, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateExplosionFor_WithResponsiblePlayer_ShouldSendPacketToAllPlayers(
        ExplosionService sut,
        LightTestPlayer[] players,
        LightTestPlayer responsiblePlayer,
        TestPacketContext context
    )
    {
        // Arrange
        var position = new Vector3(100, 200, 10);
        var type = ExplosionType.Molotov;

        // Act
        sut.CreateExplosionFor(players, position, type, responsiblePlayer);

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_EXPLOSION, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateExplosionFor_WithEmptyPlayerList_ShouldNotSendPacket(
        ExplosionService sut,
        TestPacketContext context
    )
    {
        // Arrange
        var position = new Vector3(100, 200, 10);
        var type = ExplosionType.Grenade;

        // Act
        sut.CreateExplosionFor(Enumerable.Empty<LightTestPlayer>(), position, type);

        // Assert
        context.VerifyNoPacketsSent();
    }

    [Theory]
    [AutoDomainData]
    public void CreateExplosionFor_DifferentExplosionTypes_ShouldSendPackets(
        ExplosionService sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Arrange
        var position = new Vector3(100, 200, 10);

        // Act
        sut.CreateExplosionFor([player], position, ExplosionType.Grenade);
        sut.CreateExplosionFor([player], position, ExplosionType.Rocket);
        sut.CreateExplosionFor([player], position, ExplosionType.Car);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_EXPLOSION, player, count: 3);
    }
}
