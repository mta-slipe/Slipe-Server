using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SlipeServer.Server.TestTools;
using Xunit;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;
using SlipeServer.Hosting;
using System.Net.Http;
using Moq;
using Microsoft.Extensions.Logging;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.Tests.Integration;

public class HostingTests
{
    [Fact]
    public void HostingShouldWork()
    {
        Mock<ILogger> loggerMock = new();
        var sampleService = new SampleHostedService();
        var masterServer = new MtaSaMasterServerDelegateHandler();

        TestingPlayer player;
        Element element = new();
        {
            using var hosting = new TestingServerHosting(new Configuration(), hostBuilder =>
            {
                hostBuilder.Services.AddHostedService<DefaultStartAllMtaServersHostedService>();
                hostBuilder.Services.AddSingleton(loggerMock.Object);
                hostBuilder.Services.AddHostedService(x => sampleService);
                hostBuilder.Services.AddSingleton(new HttpClient(masterServer));
                hostBuilder.Services.AddDefaultMtaServerServices();

                hostBuilder.ConfigureMtaServers(configure =>
                {
                    configure.AddDefaultPacketHandlers();
                    configure.AddDefaultBehaviours();
                });
            }, null);

            player = hosting.Server.AddFakePlayer();
            player.Client.IsConnected.Should().BeTrue();

            element.AssociateWith(hosting.Server);
        }

        element.IsDestroyed.Should().BeTrue();
        masterServer.Servers.Should().HaveCount(1);
        player.Client.IsConnected.Should().BeFalse();
        sampleService.Started.Should().BeTrue();
        sampleService.Stopped.Should().BeTrue();
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
