using Moq;
using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.PacketHandling;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using SlipeServer.Server.TestTools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Miscellaneous
{
    public class ClientPacketScopeTests
    {
        [Fact]
        public void ClientInScope_SendsPacket()
        {
            var server = new TestingServer();
            var handler = server.Instantiate<SyncQueueHandler>(QueueHandlerScalingConfig.Default, 0);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, handler);

            var player = server.AddFakePlayer();

            using var scope = new ClientPacketScope(new Client[] { player.Client });
            player.Client.SendPacket(new SetElementModelRpcPacket(player.Id, 0));

            server.NetWrapperMock.Verify(x => x.SendPacket(
                player.Address,
                It.IsAny<SetElementModelRpcPacket>()
            ), Times.Once);
        }

        [Fact]
        public void ClientOutOfScope_DoesNotSendPacket()
        {
            var server = new TestingServer();
            var handler = server.Instantiate<SyncQueueHandler>(QueueHandlerScalingConfig.Default, 0);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, handler);

            var player = server.AddFakePlayer();

            using var scope = new ClientPacketScope(new Client[] { });
            player.Client.SendPacket(new SetElementModelRpcPacket(player.Id, 0));

            server.NetWrapperMock.Verify(x => x.SendPacket(
                player.Address,
                It.IsAny<SetElementModelRpcPacket>()
            ), Times.Never);
        }

        [Fact]
        public async Task AsyncScopes_DoNotInterfere()
        {
            var server = new TestingServer();
            var handler = server.Instantiate<SyncQueueHandler>(QueueHandlerScalingConfig.Default, 0);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, handler);

            var player = server.AddFakePlayer();

            await Task.WhenAll(new Task[]
            {
                Task.Run(async() =>
                {
                    await Task.Delay(10);
                    using var scope = new ClientPacketScope(new Client[] { player.Client });
                    player.Client.SendPacket(new SetElementModelRpcPacket(player.Id, 1));
                    await Task.Delay(25);
                }),
                Task.Run(async() =>
                {
                    using var scope = new ClientPacketScope(new Client[] { });
                    await Task.Delay(25);
                    player.Client.SendPacket(new SetElementModelRpcPacket(player.Id, 2));
                })
            });

            server.NetWrapperMock.Verify(x => x.SendPacket(
                player.Address,
                It.Is<SetElementModelRpcPacket>(x => x.Model == 1)
            ), Times.Once);

            server.NetWrapperMock.Verify(x => x.SendPacket(
                player.Address,
                It.Is<SetElementModelRpcPacket>(x => x.Model == 2)
            ), Times.Never);
        }
    }
}
