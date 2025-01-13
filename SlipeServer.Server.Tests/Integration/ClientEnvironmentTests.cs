using FluentAssertions;
using FluentAssertions.Execution;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.TestTools;
using System.Numerics;
using System.Threading;
using Xunit;

namespace SlipeServer.Server.Tests.Integration;

public class ClientEnvironmentTests
{
    [Fact]
    public void SettingGettingPositionShouldWork()
    {
        var server = new TestingServer();
        server.AddDefaultPacketHandlers();
        var player = server.AddFakePlayer();
        var clientEnvironment = new ClientEnvironment(server, player);

        using var monitor = clientEnvironment.ClientPlayer.Monitor();
        player.Position = new Vector3(3, 3, 3);
        var clientPositionWas = player.Position;
        clientEnvironment.ClientPlayer.SetPosition(new Vector3(5, 0, 0));

        var waitHandle = new AutoResetEvent(false);
        void handlePositionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
        {
            waitHandle.Set();
        }
        player.PositionChanged += handlePositionChanged;
        clientEnvironment.ClientPlayer.SynchronizeWithServer();

        waitHandle.WaitOne(1000);

        using var _ = new AssertionScope();

        clientPositionWas.Should().Be(new Vector3(3, 3, 3));
        player.Position.Should().Be(new Vector3(5, 0, 0));
    }
}
