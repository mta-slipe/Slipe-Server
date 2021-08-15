using Moq;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware;
using SlipeServer.Server.Repositories;
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

            server.HandlePacket(player1, PacketId.PACKET_ID_PLAYER_PURESYNC, this.testPacket);

            while (handler.QueuedPacketCount > 0)
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

            while (handler.QueuedPacketCount > 0)
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

            var player1 = server.AddFakePlayer();
            var player2 = server.AddFakePlayer();

            var elementRepositoryMock = new Mock<IElementRepository>();
            elementRepositoryMock.Setup(x => x.GetAll()).Returns(new Player[] { player1, player2 });
            var elementRepository = elementRepositoryMock.Object;

            var middleware = new BasicSyncHandlerMiddleware<PlayerPureSyncPacket>(elementRepository);
            var handler = server.Instantiate<SyncQueueHandler>(QueueHandlerScalingConfig.Default, 0, elementRepository, middleware);

            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, handler);

            server.HandlePacket(player1, PacketId.PACKET_ID_PLAYER_PURESYNC, testPacket);

            while (handler.QueuedPacketCount > 0)
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
