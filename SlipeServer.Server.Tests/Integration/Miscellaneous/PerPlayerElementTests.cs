using SlipeServer.Packets.Structs;
using SlipeServer.Server.Elements;
using SlipeServer.Server.TestTools;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Miscellaneous;

public class PerPlayerElementTests
{
    [Fact]
    public void AssociateWith_CreatesElementForRelevantPlayer()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        server.ResetPacketCountVerification();

        var element = new Element() { Id = (ElementId)100 };
        element.AssociateWith(player1);

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player2, count: 0);
    }

    [Fact]
    public void AssociatedElement_RelaysChangesToRelevantPlayer()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        var element = new Element() { Id = (ElementId)100 };
        element.AssociateWith(player1);
        element.Alpha = 200;

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_ELEMENT_RPC, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_ELEMENT_RPC, player2, count: 0);
    }

    [Fact]
    public void AssociatedElement_DestroyTriggersEntityRemovePacketToRelevantPlayer()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        var element = new Element() { Id = (ElementId)100 };
        element.AssociateWith(player1);
        element.Destroy();

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player2, count: 0);
    }

    [Fact]
    public void AssociatedElement_RemoveFromTriggersEntityRemovePacketToRelevantPlayer()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        var element = new Element() { Id = (ElementId)100 };
        element.AssociateWith(player1);
        element.RemoveFrom(player1);

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player2, count: 0);
    }

    [Fact]
    public void AssociatedElement_RemoveFromNoLongerTriggersUpdatePackets()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        var element = new Element() { Id = (ElementId)100 };
        element.AssociateWith(player1);
        element.RemoveFrom(player1);
        element.Alpha = 200;

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_ELEMENT_RPC, player1, count: 0);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_LUA_ELEMENT_RPC, player2, count: 0);
    }
}
