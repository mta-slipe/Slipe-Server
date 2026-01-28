using FluentAssertions;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class ClientConsoleTests
{
    [Theory]
    [AutoDomainData]
    public void Output_ShouldBroadcastToAllPlayers(
        ClientConsole sut,
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
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CONSOLE_ECHO, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_SinglePlayer_ShouldSendPacketToPlayer(
        ClientConsole sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.OutputTo(player, "Test message");

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CONSOLE_ECHO, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_MultipleTimes_ShouldSendMultiplePacketsToPlayer(
        ClientConsole sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.OutputTo(player, "Message 1");
        sut.OutputTo(player, "Message 2");
        sut.OutputTo(player, "Message 3");

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CONSOLE_ECHO, player, count: 3);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_DifferentPlayers_ShouldSendPacketToEachPlayer(
        ClientConsole sut,
        LightTestPlayer[] players,
        TestPacketContext context
    )
    {
        // Act
        foreach (var player in players)
            sut.OutputTo(player, "Test message");

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CONSOLE_ECHO, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_WithVariousMessages_ShouldSendPackets(
        ClientConsole sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.OutputTo(player, string.Empty);
        sut.OutputTo(player, "Test");
        var longMessage = new string('A', 1000);
        sut.OutputTo(player, longMessage);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CONSOLE_ECHO, player, count: 3);
    }
}

