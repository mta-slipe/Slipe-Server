using Moq;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.TestTools;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Miscellaneous;

public class BroadcastPacketTests
{
    [Fact]
    public void BroadcastPacket_SendsPacketToAllPlayers()
    {
        var server = new TestingServer();

        var player = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        server.BroadcastPacket(new SetElementModelRpcPacket(player.Id, 0));

        server.VerifyPacketSent(PacketId.PACKET_ID_LUA_ELEMENT_RPC, player);
        server.VerifyPacketSent(PacketId.PACKET_ID_LUA_ELEMENT_RPC, player2);
    }
}
