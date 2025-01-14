using FluentAssertions;
using FluentAssertions.Execution;
using SlipeServer.Server.TestTools;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration;

public class ClientPlayerTests
{
    [Fact]
    public void SettingGettingPositionShouldWork()
    {
        var server = new TestingServer();
        server.AddDefaultPacketHandlers();
        var player1 = server.AddFakePlayer();
        player1.Name = "player1";
        var clientPlayer1 = server.CreateClientPlayer(player1);
        var player2 = server.AddFakePlayer();
        player2.Name = "player2";
        var clientPlayer2 = server.CreateClientPlayer(player2);

        using var monitor = clientPlayer1.Monitor();
        player1.Position = new Vector3(3, 3, 3);
        var wasClientPosition = player1.Position;
        clientPlayer1.SetPosition(new Vector3(5, 0, 0));

        clientPlayer1.SynchronizeWithServer();
        server.FlushPacketQueueHandler();

        using var _ = new AssertionScope();

        wasClientPosition.Should().Be(new Vector3(3, 3, 3));
        player1.Position.Should().Be(new Vector3(5, 0, 0));
        clientPlayer2.ElementCollection.Get(player1.Id).Position.Should().Be(new Vector3(5, 0, 0));
    }
}
