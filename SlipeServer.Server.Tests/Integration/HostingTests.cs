using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlipeServer.Server.TestTools;
using Xunit;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;
using System.Net.Http;
using SlipeServer.Server.Elements;
using System;
using System.Linq;

namespace SlipeServer.Server.Tests.Integration;

public class HostingTests
{
    [Fact]
    public void HostingShouldWork()
    {
        var sampleService = new SampleHostedService();
        var masterServer = new MtaSaMasterServerDelegateHandler();

        TestingPlayer player;
        Element element = new();
        MtaServer server;
        {
            using var hosting = new TestingServerHosting(new Configuration(), hostBuilder =>
            {
                hostBuilder.Services.AddHostedService(x => sampleService);
                hostBuilder.Services.AddSingleton(new HttpClient(masterServer));
                hostBuilder.Services.AddDefaultMtaServerServices();

            }, null);

            server = hosting.Server;

            player = hosting.Server.AddFakePlayer();
            player.Client.IsConnected.Should().BeTrue();
            server.IsRunning.Should().BeTrue();

            element.AssociateWith(hosting.Server);
        }

        element.IsDestroyed.Should().BeTrue();
        //masterServer.Servers.Should().HaveCount(1);
        player.Client.IsConnected.Should().BeFalse();
        sampleService.Started.Should().BeTrue();
        sampleService.Stopped.Should().BeTrue();
        server.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void HostingWithDIPlayerShouldWork()
    {
        using var hosting = new TestingServerHosting<DIPlayer>(new Configuration(), applicationBuilder =>
        {
            applicationBuilder.Services.AddSingleton<SampleService>();
        });

        var player = hosting.Server.AddFakePlayer();

        var serviceA = hosting.GetRequiredService<SampleService>();
        var serviceB = hosting.Server.GetRequiredService<SampleService>();

        player.Should().NotBeNull();
        serviceA.Should().Be(serviceB);
    }

    [Fact]
    public void HostingShouldFailToStart()
    {
        var createHosting = () => new TestingServerHosting<DIPlayer>(new Configuration(), applicationBuilder =>
        {
            applicationBuilder.Services.AddHostedService<MalfunctionService>();
        });

        var aggregateExceptions = createHosting.Should().Throw<AggregateException>().And.InnerExceptions;
        aggregateExceptions.Should().HaveCount(1);
        aggregateExceptions.First().Should().BeOfType<Exception>();
        aggregateExceptions.First().Message.Should().Be("oops");
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

public class SampleService;

public class MalfunctionService : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => throw new Exception("oops");
    public Task StopAsync(CancellationToken cancellationToken) => throw new Exception("oops");
}

public class DIPlayer : Player
{
    private readonly IServiceProvider serviceProvider;

    public DIPlayer(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }
}
