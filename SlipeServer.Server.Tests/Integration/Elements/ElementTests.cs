using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.Elements;
using SlipeServer.Server.TestTools;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Elements;
public class ElementTests
{
    [Fact]
    public void PositionChangeSendsSetElementPositionPacket()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<ElementPacketBehaviour>();

        var element = new Element().AssociateWith(server);

        element.Position = Vector3.One;

        var expectedPacketBuilder = new PacketBuilder();
        expectedPacketBuilder.Write((byte)ElementRpcFunction.SET_ELEMENT_POSITION);
        expectedPacketBuilder.WriteElementId(element.Id);
        expectedPacketBuilder.Write(Vector3.One);
        expectedPacketBuilder.Write(element.TimeContext);
        expectedPacketBuilder.Write((byte)0);

        server.VerifyPacketSent(PacketId.PACKET_ID_LUA_ELEMENT_RPC, player, expectedPacketBuilder.Build());
    }

    [Fact]
    public void ElementCreated_SendsEntityAddPacket()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<ElementPacketBehaviour>();

        new Element().AssociateWith(server);

        server.VerifyPacketSent(PacketId.PACKET_ID_ENTITY_ADD, player);
    }

    [Fact]
    public void ElementDestroy_SendsEntityRemovePacket()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<ElementPacketBehaviour>();

        var element = new Element().AssociateWith(server);
        element.Destroy();

        server.VerifyPacketSent(PacketId.PACKET_ID_ENTITY_REMOVE, player);
    }
}
