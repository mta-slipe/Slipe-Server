using Moq;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.PacketHandling;
using SlipeServer.Server.TestTools;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Miscellaneous
{
    public class BroadcastPacketTests
    {
        [Fact]
        public void BroadcastPacket_SendsPacketToAllPlayers()
        {
            var server = new TestingServer();

            var player = server.AddFakePlayer();
            var player2 = server.AddFakePlayer();

            server.BroadcastPacket(new SetElementModelRpcPacket(player.Id, 0));

            server.NetWrapperMock.Verify(x => x.SendPacket(
                player.Address,
                PacketId.PACKET_ID_LUA_ELEMENT_RPC,
                It.IsAny<byte[]>(),
                It.IsAny<PacketPriority>(),
                It.IsAny<PacketReliability>()
            ), Times.Once);
            server.NetWrapperMock.Verify(x => x.SendPacket(
                player2.Address,
                PacketId.PACKET_ID_LUA_ELEMENT_RPC,
                It.IsAny<byte[]>(),
                It.IsAny<PacketPriority>(),
                It.IsAny<PacketReliability>()
            ), Times.Once);
        }
    }
}
