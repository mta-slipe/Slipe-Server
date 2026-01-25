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


    [Fact]
    public void AssociateWith_CreatesElementOnlyForNewlyAssociatedPlayer()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        server.ResetPacketCountVerification();

        var element = new Element() { Id = (ElementId)100 };

        element.AssociateWith(player1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player2, count: 0);

        element.AssociateWith(player2);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player2, count: 1);
    }


    [Fact]
    public void RemoveFrom_DestroysElementOnlyForNewlyRemovedPlayer()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        server.ResetPacketCountVerification();

        var element = new Element() { Id = (ElementId)100 };

        element.AssociateWith(player1);
        element.AssociateWith(player2);

        element.RemoveFrom(player1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player2, count: 0);

        element.RemoveFrom(player2);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player2, count: 1);
    }


    [Fact]
    public void AssociateWithServer_CreatesElementOnlyForNewlyAssociatedPlayer()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        server.ResetPacketCountVerification();

        var element = new Element() { Id = (ElementId)100 };

        element.AssociateWith(player1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player2, count: 0);

        element.AssociateWith(server);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player2, count: 1);
    }


    [Fact]
    public void RemoveFromServer_DestroysElementOnlyForNewlyRemovedPlayer()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();
        var player2 = server.AddFakePlayer();

        server.ResetPacketCountVerification();

        var element = new Element() { Id = (ElementId)100 };

        element.AssociateWith(server);
        element.AssociateWith(player2);

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player2, count: 1);

        element.RemoveFrom(server);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player2, count: 0);

        element.RemoveFrom(player2);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player1, count: 1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player2, count: 1);
    }


    [Fact]
    public void AssociateWithServerWhenAlreadyAssociatedWithPlayer_CreatesElementOnlyOnce()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();

        server.ResetPacketCountVerification();

        var element = new Element() { Id = (ElementId)100 };

        element.AssociateWith(server);
        element.AssociateWith(player1);

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player1, count: 1);
    }


    [Fact]
    public void AssociateWithPlayerWhenAlreadyAssociatedWithServer_CreatesElementOnlyOnce()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();

        server.ResetPacketCountVerification();

        var element = new Element() { Id = (ElementId)100 };

        element.AssociateWith(player1);
        element.AssociateWith(server);

        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_ADD, player1, count: 1);
    }


    [Fact]
    public void RemoveFromPlayer_WhenStillAssociatedWithServer_DoesNotSendRemovePacket()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();

        server.ResetPacketCountVerification();

        var element = new Element() { Id = (ElementId)100 };

        element.AssociateWith(server);
        element.AssociateWith(player1);

        element.RemoveFrom(player1);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player1, count: 0);
    }


    [Fact]
    public void RemoveFromServer_WhenStillAssociatedWithPlayer_DoesNotSendRemovePacket()
    {
        var server = new TestingServer();

        var player1 = server.AddFakePlayer();

        server.ResetPacketCountVerification();

        var element = new Element() { Id = (ElementId)100 };

        element.AssociateWith(server);
        element.AssociateWith(player1);

        element.RemoveFrom(server);
        server.VerifyPacketSent(Packets.Enums.PacketId.PACKET_ID_ENTITY_REMOVE, player1, count: 0);
    }
}
