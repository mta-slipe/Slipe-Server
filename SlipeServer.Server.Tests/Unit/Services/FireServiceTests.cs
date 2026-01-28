using FluentAssertions;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using System.Linq;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class FireServiceTests
{
    [Theory]
    [AutoDomainData]
    public void CreateFire_ShouldBroadcastToAllPlayers(
        FireService sut,
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

        // Act
        sut.CreateFire(position);

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_FIRE, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateFireFor_SinglePlayer_ShouldSendPacketToPlayer(
        FireService sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Arrange
        var position = new Vector3(100, 200, 10);

        // Act
        sut.CreateFireFor([player], position);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_FIRE, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateFireFor_MultiplePlayers_ShouldSendPacketToAllPlayers(
        FireService sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        // Arrange
        var position = new Vector3(100, 200, 10);

        // Act
        sut.CreateFireFor(players, position);

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_FIRE, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateFireFor_WithCustomSize_ShouldSendPacketToAllPlayers(
        FireService sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        // Arrange
        var position = new Vector3(100, 200, 10);
        var size = 3.5f;

        // Act
        sut.CreateFireFor(players, position, size);

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_FIRE, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateFireFor_WithResponsiblePlayer_ShouldSendPacketToAllPlayers(
        FireService sut,
        LightTestPlayer[] players,
        LightTestPlayer responsiblePlayer,
        TestPacketContext context
    )
    {
        // Arrange
        var position = new Vector3(100, 200, 10);

        // Act
        sut.CreateFireFor(players, position, responsiblePlayer: responsiblePlayer);

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_FIRE, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void CreateFireFor_WithEmptyPlayerList_ShouldNotSendPacket(
        FireService sut,
        TestPacketContext context
    )
    {
        // Arrange
        var position = new Vector3(100, 200, 10);

        // Act
        sut.CreateFireFor(Enumerable.Empty<LightTestPlayer>(), position);

        // Assert
        context.VerifyNoPacketsSent();
    }

    [Theory]
    [AutoDomainData]
    public void CreateFireFor_MultipleFires_ShouldSendMultiplePackets(
        FireService sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Arrange
        var position1 = new Vector3(100, 200, 10);
        var position2 = new Vector3(150, 250, 15);
        var position3 = new Vector3(200, 300, 20);

        // Act
        sut.CreateFireFor([player], position1);
        sut.CreateFireFor([player], position2);
        sut.CreateFireFor([player], position3);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_FIRE, player, count: 3);
    }
}
