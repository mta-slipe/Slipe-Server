using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.TestTools;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Services;

public class TestResourceStartPacketHandler : IPacketHandler<ResourceStartPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_RESOURCE_START;

    public TestResourceStartPacketHandler()
    {
    }

    public void HandlePacket(IClient client, ResourceStartPacket packet)
    {
        client.Player.TriggerResourceStarted(packet.NetId);
    }
}

public class ResourceA : Resource
{
    public ResourceA(MtaServer server) : base(server, server.RootElement, "ResourceA") { }
}

public class ResourceB : Resource
{
    public ResourceB(MtaServer server) : base(server, server.RootElement, "ResourceB") { }
}

public class TestingServerResourceService
{
    public TestingServer Server { get; }
    public ResourceA ResourceA { get; }
    public ResourceB ResourceB { get; }

    public TestingServerResourceService()
    {
        this.Server = new TestingServer((Configuration?)null, builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IResourceProvider>();
                services.AddSingleton<TestResourceProvider>();
                services.AddSingleton<IResourceProvider>(x => x.GetRequiredService<TestResourceProvider>());
            });
        });

        this.Server.RegisterPacketHandler<TestResourceStartPacketHandler, ResourceStartPacket>();

        var resourceProvider = this.Server.GetRequiredService<TestResourceProvider>();
        this.ResourceA = new ResourceA(this.Server);
        this.Server.AddAdditionalResource(this.ResourceA, []);
        resourceProvider.AddResource(this.ResourceA);
        this.ResourceB = new ResourceB(this.Server);
        this.Server.AddAdditionalResource(this.ResourceB, []);
        resourceProvider.AddResource(this.ResourceB);
    }
}

public class ResourceServiceTests : IClassFixture<TestingServerResourceService>
{
    private readonly TestingServer server;
    private readonly ResourceService resourceService;
    private readonly ResourceA resourceA;
    private readonly ResourceB resourceB;

    public ResourceServiceTests(TestingServerResourceService testingServerResourceService)
    {
        this.server = testingServerResourceService.Server;

        this.resourceService = this.server.CreateInstance<ResourceService>();
        this.resourceService.AddStartResource("ResourceA");
        this.resourceService.AddStartResource("ResourceB");

        this.resourceA = testingServerResourceService.ResourceA;
        this.resourceB = testingServerResourceService.ResourceB;
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task ResourceServiceShouldWork(bool parallel)
    {
        var player = this.server.AddFakePlayer();
        using var monitor = this.resourceService.Monitor();

        await this.resourceService.StartResourcesForPlayer(player, parallel);

        using var _ = new AssertionScope();
        monitor.OccurredEvents.Select(x => x.EventName).Should().BeEquivalentTo(["Started", "Started", "AllStarted"]);
        this.server.VerifyResourceStartedPacketSent(player, this.resourceA);
        this.server.VerifyResourceStartedPacketSent(player, this.resourceB);
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task DisconnectingPlayerShouldStopResourceStarting(bool parallel)
    {
        using var monitor = this.resourceService.Monitor();

        var player = this.server.AddFakePlayer();

        void handleStarted(Resource arg1, Player arg2)
        {
            player.TriggerDisconnected(Enums.QuitReason.Quit);
        }

        this.resourceService.Started += handleStarted;

        var act = async () => await this.resourceService.StartResourcesForPlayer(player, parallel);

        using var _ = new AssertionScope();
        (await act.Should().ThrowAsync<AggregateException>())
            .Which.InnerExceptions.Should()
            .SatisfyRespectively(
                first =>
                {
                    first.Should().BeOfType<OperationCanceledException>();
                });

        monitor.OccurredEvents.Select(x => x.EventName).Should().BeEquivalentTo(["Started"]);

        if (parallel)
        {
            this.server.VerifyResourceStartedPacketSent(player, this.resourceA);
            this.server.VerifyResourceStartedPacketSent(player, this.resourceB);
        }
        else
        {
            this.server.VerifyResourceStartedPacketSent(player, this.resourceA);
            this.server.VerifyResourceStartedPacketSent(player, this.resourceB, 0);
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task ExceptionThrownInStartedEventShouldBeHandledProperly(bool parallel)
    {
        using var monitor = this.resourceService.Monitor();

        var player = this.server.AddFakePlayer();

        void handleStarted(Resource resource, Player player)
        {
            throw new Exception("oops");
        }
        
        void handleAllStarted(Player player)
        {
            throw new Exception("oops");
        }

        this.resourceService.Started += handleStarted;
        this.resourceService.AllStarted += handleAllStarted;

        var act = async () => await this.resourceService.StartResourcesForPlayer(player, parallel);

        using var _ = new AssertionScope();
        (await act.Should().ThrowAsync<AggregateException>())
            .Which.InnerExceptions.Should().HaveCount(3);

        monitor.OccurredEvents.Select(x => x.EventName).Should().BeEquivalentTo(["Started", "Started", "AllStarted"]);

        this.server.VerifyResourceStartedPacketSent(player, this.resourceA);
        this.server.VerifyResourceStartedPacketSent(player, this.resourceB);
    }


}
