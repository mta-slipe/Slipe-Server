using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.PacketHandling.Handlers;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.TestTools;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Services;

public class ClientPlayerResourceStartedPacketHandler : IPacketHandler<ResourceStartPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_RESOURCE_START;

    public ClientPlayerResourceStartedPacketHandler()
    {
    }

    public void HandlePacket(IClient client, ResourceStartPacket packet)
    {
        client.Player.TriggerResourceStarted(packet.NetId);
    }
}

public class ResourceServiceTests
{
    class ResourceA : Resource
    {
        public ResourceA(MtaServer server) : base(server, server.RootElement, "ResourceA") { }
    }
    
    class ResourceB : Resource
    {
        public ResourceB(MtaServer server) : base(server, server.RootElement, "ResourceB") { }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task ResourceServiceShouldWork(bool parallel)
    {
        var server = new TestingServer((Configuration?)null, builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IResourceProvider>();
                services.AddSingleton<TestResourceProvider>();
                services.AddSingleton<IResourceProvider>(x => x.GetRequiredService<TestResourceProvider>());
            });
        });

        server.RegisterPacketHandler<ClientPlayerResourceStartedPacketHandler, ResourceStartPacket>();

        var resourceProvider = server.GetRequiredService<TestResourceProvider>();
        var resourceA = new ResourceA(server);
        server.AddAdditionalResource(resourceA, []);
        resourceProvider.AddResource(resourceA);
        var resourceB = new ResourceB(server);
        server.AddAdditionalResource(resourceB, []);
        resourceProvider.AddResource(resourceB);

        var resourceService = server.CreateInstance<ResourceService>();
        using var monitor = resourceService.Monitor();

        resourceService.StartResource("ResourceA");
        resourceService.StartResource("ResourceB");

        var player = server.AddFakePlayer();
        await resourceService.StartResourcesForPlayer(player, parallel);

        using var _ = new AssertionScope();
        monitor.OccurredEvents.Select(x => x.EventName).Should().BeEquivalentTo(["Started", "Started", "AllStarted"]);
        server.VerifyResourceStartedPacketSent(player, resourceA);
        server.VerifyResourceStartedPacketSent(player, resourceB);
    }
}
