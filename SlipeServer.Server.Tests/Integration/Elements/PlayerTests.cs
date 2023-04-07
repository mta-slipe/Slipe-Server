using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Loggers;
using SlipeServer.Server.PacketHandling.Handlers.Player;
using SlipeServer.Server.PacketHandling.Handlers.QueueHandlers;
using SlipeServer.Server.Resources;
using SlipeServer.Server.TestTools;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Elements;

public class TestResource : Resource
{
    public TestResource(MtaServer server)
        : base(server, server.GetRequiredService<RootElement>(), "Test")
    {
    }
}

public class PlayerTests
{
    [Fact]
    public async void WastedPacketReceived_Relays_wasted_packet()
    {
        var server = new TestingServer();
        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        var handler = server.Instantiate<PlayerWastedPacketHandler>();
        var queueHandler = new ScalingPacketQueueHandler<PlayerWastedPacket>(new NullLogger(), handler);
        server.RegisterPacketHandler(PacketId.PACKET_ID_PLAYER_WASTED, queueHandler);

        server.EnqueuePacketToClient(player1.Client, PacketId.PACKET_ID_PLAYER_WASTED, new byte[] {
            240, 232, 192, 224, 6, 163, 149, 228, 3, 40, 116, 12, 119, 72, 123, 7, 0,
        });

        await queueHandler.GetPulseTask();

        server.VerifyPacketSent(PacketId.PACKET_ID_PLAYER_WASTED, player1, count: 1);
        server.VerifyPacketSent(PacketId.PACKET_ID_PLAYER_WASTED, player2, count: 1);
    }

    [Fact]
    public void KillMethod_Relays_wasted_packet()
    {
        var server = new TestingServer();
        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        player1.Kill();

        server.VerifyPacketSent(PacketId.PACKET_ID_PLAYER_WASTED, player1, count: 1);
        server.VerifyPacketSent(PacketId.PACKET_ID_PLAYER_WASTED, player2, count: 1);
    }

    [Fact]
    public void StartForMethod_Relays_startfor_packet()
    {
        var server = new TestingServer(null, builder =>
        {
            builder.AddBuildStep(server =>
            {
                var resource = new TestResource(server);
                server.AddAdditionalResource(resource, new Dictionary<string, byte[]>());
            });
        });

        var player = server.AddFakePlayer();
        var resource = server.GetAdditionalResource<TestResource>();

        resource.StartFor(player);

        server.VerifyPacketSent(PacketId.PACKET_ID_RESOURCE_START, player, count: 1);
    }

    [Fact]
    public async Task StartForAsyncMethod_Relays_startfor_packet()
    {
        var server = new TestingServer(null, builder =>
        {
            builder.AddBuildStep(server =>
            {
                var resource = new TestResource(server);
                server.AddAdditionalResource(resource, new Dictionary<string, byte[]>());
            });
        });

        var player = server.AddFakePlayer();
        var resource = server.GetAdditionalResource<TestResource>();

        var wait = new TaskCompletionSource();
        server.PacketSent += (uint address, ushort version, Packet packet) =>
        {
            if(packet is ResourceStartPacket startPacket)
            {
                wait.SetResult();
            }
            else
            {
                wait.SetException(new System.Exception());
            }
        };

        _ = Task.Run(async () =>
        {
            await resource.StartForAsync(player);
        });

        await wait.Task;

        server.VerifyPacketSent(PacketId.PACKET_ID_RESOURCE_START, player, count: 1);
    }
}
