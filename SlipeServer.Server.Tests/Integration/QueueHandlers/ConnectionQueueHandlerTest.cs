using Moq;
using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.Join;
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
    public class ConnectionQueueHandlerTest
    {
        [Fact]
        public async Task ConnectionFlowTest()
        {
            var server = new TestingServer();
            var address = server.GenerateBinaryAddress();

            server.NetWrapperMock
                .Setup(x => x.GetClientSerialExtraAndVersion(address))
                .Returns(new Tuple<string, string, string>("", "", ""));

            var connectionHandler = server.Instantiate<ConnectionQueueHandler>(0, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_JOIN, connectionHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_JOINDATA, connectionHandler);

            server.HandlePacket(address, PacketId.PACKET_ID_PLAYER_JOIN, new byte[] { });

            await connectionHandler.GetPulseTask();

            server.NetWrapperMock.Verify(x => x.SendPacket(
                address,
                It.Is<ModNamePacket>(x => x.Name == "deathmatch")
            ));

            server.HandlePacket(address, PacketId.PACKET_ID_PLAYER_JOINDATA, new PlayerJoinDataPacket().Write());

            await connectionHandler.GetPulseTask();

            server.NetWrapperMock.Verify(x => x.SendPacket(
                address,
                It.IsAny<JoinCompletePacket>()
            ));
        }
    }
}
