using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlipeServer.Server.TestTools;
using Xunit;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.Tests.Integration;

public class HostingTests
{
    [Fact]
    public void HostingShouldWork()
    {
        var sampleService = new SampleHostedService();

        TestingPlayer player;
        {
            using var hosting = new TestingServerHosting(hostBuilder =>
            {
                hostBuilder.Services.AddHostedService(x => sampleService);
            }, null);

            player = hosting.Server.AddFakePlayer();
            player.Client.IsConnected.Should().BeTrue();
        }

        player.Client.IsConnected.Should().BeFalse();
        sampleService.Started.Should().BeTrue();
        sampleService.Stopped.Should().BeTrue();
    }

    [Fact]
    public void ClientInterfaceShouldWork()
    {
        using var hosting = new TestingServerHosting();

        var player = hosting.Server.AddFakePlayer();
        var clientInterface = new ClientEnvironment(player);

        player.Position = new System.Numerics.Vector3(3, 3, 3);

        clientInterface.Position.Should().Be(player.Position);
    }
}

public class SampleHostedService : IHostedService
{
    public bool Started { get; private set; }
    public bool Stopped { get; private set; }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.Started = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.Stopped = true;
        return Task.CompletedTask;
    }
}
