using FluentAssertions;
using SlipeServer.Server.TestTools;
using Xunit;

namespace SlipeServer.Server.Tests.Integration;

public class ClientEnvironmentTests
{
    [Fact]
    public void SettingGettingPositionShouldWork()
    {
        var server = new TestingServer();

        var player = server.AddFakePlayer();
        var clientInterface = new ClientEnvironment(player);

        player.Position = new System.Numerics.Vector3(3, 3, 3);

        clientInterface.Position.Should().Be(player.Position);
    }
}
