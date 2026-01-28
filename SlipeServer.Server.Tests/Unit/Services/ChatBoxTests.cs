using SlipeServer.Packets.Enums;
using SlipeServer.Server.Services;
using SlipeServer.Server.Tests.Tools;
using System.Drawing;
using Xunit;

namespace SlipeServer.Server.Tests.Unit.Services;

public class ChatBoxTests
{
    [Theory]
    [AutoDomainData]
    public void Output_ShouldBroadcastToAllPlayers(
        ChatBox sut,
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
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CHAT_ECHO, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void Clear_ShouldBroadcastToAllPlayers(
        ChatBox sut,
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
        sut.Clear();

        // Assert
        foreach (var player in players)
            context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CHAT_CLEAR, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_SinglePlayer_ShouldSendPacketToPlayer(
        ChatBox sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.OutputTo(player, "Test message");

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CHAT_ECHO, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_WithAllParameters_ShouldSendPacketToPlayer(
        ChatBox sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.OutputTo(player, "Test message", Color.Blue, true, ChatEchoType.Internal);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CHAT_ECHO, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void ClearFor_ShouldSendPacketToPlayer(
        ChatBox sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.ClearFor(player);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CHAT_CLEAR, player, count: 1);
    }

    [Theory]
    [AutoDomainData]
    public void OutputTo_DifferentParameters_ShouldSendMultiplePackets(
        ChatBox sut,
        LightTestPlayer player,
        TestPacketContext context
    )
    {
        // Act
        sut.OutputTo(player, "Message 1");
        sut.OutputTo(player, "Message 2", Color.Red);
        sut.OutputTo(player, "Message 3", isColorCoded: true);

        // Assert
        context.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_CHAT_ECHO, player, count: 3);
    }
}

