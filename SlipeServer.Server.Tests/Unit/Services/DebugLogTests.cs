using FluentAssertions;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using System.Drawing;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class DebugLogTests
{
    [Theory]
    [AutoDomainData]
    public void Output_ShouldBroadcastToAllPlayers(
        DebugLog sut,
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
        sut.Output("Test message");

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_DEBUG_ECHO, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_SinglePlayer_ShouldSendPacketToPlayer(
        DebugLog sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.OutputTo(player, "Test message");

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_DEBUG_ECHO, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_WithDebugLevel_ShouldSendPacketToPlayer(
        DebugLog sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.OutputTo(player, "Test message", DebugLevel.Warning);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_DEBUG_ECHO, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_WithAllParameters_ShouldSendPacketToPlayer(
        DebugLog sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.OutputTo(player, "Test message", DebugLevel.Error, Color.Blue);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_DEBUG_ECHO, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_MultiplePlayers_ShouldSendPacketToEachPlayer(
        DebugLog sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        // Act
        foreach (var player in players)
            sut.OutputTo(player, "Test message");

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_DEBUG_ECHO, player, count: 1);
    }
}



