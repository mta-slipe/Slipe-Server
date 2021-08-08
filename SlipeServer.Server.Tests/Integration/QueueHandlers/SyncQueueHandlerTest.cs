using Moq;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using SlipeServer.Server.TestTools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.QueueHandlers
{
    public class SyncQueueHandlerTest
    {
        private readonly byte[] testPacket = new byte[]
        {
            0, 0, 0, 0, 0, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 3, 32, 5, 125, 221, 62, 143, 24, 0, 0,
        };

        [Fact]
        public async Task SyncHandlerSendsReturnSync()
        {
            var server = new TestingServer();
            var handler = server.Instantiate<SyncQueueHandler>(QueueHandlerScalingConfig.Default, 0);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, handler);

            var player1 = server.AddFakePlayer();
            var player2 = server.AddFakePlayer();

            server.HandlePacket(player1, PacketId.PACKET_ID_PLAYER_PURESYNC, testPacket);

            await handler.GetPulseTask();


            server.NetWrapperMock.Verify(x => x.SendPacket(
                player1.Address,
                It.IsAny<ReturnSyncPacket>()
            ));
        }

        [Fact]
        public async Task SyncHandlerRelaysSync()
        {
            var server = new TestingServer();
            var handler = server.Instantiate<SyncQueueHandler>(QueueHandlerScalingConfig.Default, 0);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, handler);

            var player1 = server.AddFakePlayer();
            var player2 = server.AddFakePlayer();

            server.HandlePacket(player1, PacketId.PACKET_ID_PLAYER_PURESYNC, testPacket);

            await handler.GetPulseTask();

            server.NetWrapperMock.Verify(x => x.SendPacket(
                player2.Address,
                PacketId.PACKET_ID_PLAYER_PURESYNC,
                It.IsAny<byte[]>(),
                It.IsAny<PacketPriority>(),
                It.IsAny<PacketReliability>()
            ));
        }

        [Fact]
        public async Task SyncHandlerDoesNotRelaySyncBack()
        {
            var server = new TestingServer();
            var handler = server.Instantiate<SyncQueueHandler>(QueueHandlerScalingConfig.Default, 0);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, handler);

            var player1 = server.AddFakePlayer();
            var player2 = server.AddFakePlayer();

            server.HandlePacket(player1, PacketId.PACKET_ID_PLAYER_PURESYNC, testPacket);

            await handler.GetPulseTask();

            server.NetWrapperMock.Verify(x => x.SendPacket(
                player1.Address,
                PacketId.PACKET_ID_PLAYER_PURESYNC,
                It.IsAny<byte[]>(),
                It.IsAny<PacketPriority>(),
                It.IsAny<PacketReliability>()
            ), Times.Never);
        }
    }
}
