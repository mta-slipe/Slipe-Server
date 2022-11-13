using FluentAssertions;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Definitions.Lua.ElementRpc;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
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

    [Fact]
    public void SetData_ReturnsValueWhenGetting()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<CustomDataBehaviour>();

        var element = new Element().AssociateWith(server);
        element.SetData("Foo", 5, DataSyncType.Subscribe);

        var result = element.GetData("Foo");

        result.Should().Be((LuaValue)5);
    }

    [Fact]
    public void GetData_ReturnsNull_WhenSetToLuaNil()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<CustomDataBehaviour>();

        var element = new Element().AssociateWith(server);
        element.SetData("Foo", new LuaValue(), DataSyncType.Subscribe);

        var result = element.GetData("Foo");

        result.Should().Be(null);
    }

    [Fact]
    public void SetData_Local_DoesNotSendPacket()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<CustomDataBehaviour>();

        var element = new Element().AssociateWith(server);
        element.SetData("Foo", 5, DataSyncType.Local);

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.SET_ELEMENT_DATA, player, count: 0);
    }

    [Fact]
    public void SetData_Broadcast_SendsPacket()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<CustomDataBehaviour>();

        var element = new Element().AssociateWith(server);
        element.SetData("Foo", 5, DataSyncType.Broadcast);

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.SET_ELEMENT_DATA, player);
    }

    [Fact]
    public void SetData_SubscribeWithNoSubscriptions_DoesNotSendPacket()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<CustomDataBehaviour>();

        var element = new Element().AssociateWith(server);
        element.SetData("Foo", 5, DataSyncType.Subscribe);

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.SET_ELEMENT_DATA, player, count: 0);
    }

    [Fact]
    public void SetData_SubscribeWithSubscriber_SendsPacket()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<CustomDataBehaviour>();

        var element = new Element().AssociateWith(server);
        element.SubscribeToData(player, "Foo");
        element.SetData("Foo", 5, DataSyncType.Subscribe);

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.SET_ELEMENT_DATA, player);
    }

    [Fact]
    public void SetData_SubscribeAfterUnsubscribing_DoesNotSendPacket()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<CustomDataBehaviour>();

        var element = new Element().AssociateWith(server);
        element.SubscribeToData(player, "Foo");
        element.UnsubscribeFromData(player, "Foo");
        element.SetData("Foo", 5, DataSyncType.Subscribe);

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.SET_ELEMENT_DATA, player, count: 0);
    }

    [Fact]
    public void SetData_SubscribeAfterUnsubscribingFromAll_DoesNotSendPacket()
    {
        var server = new TestingServer();
        var player = server.AddFakePlayer();
        server.Instantiate<CustomDataBehaviour>();

        var element = new Element().AssociateWith(server);
        element.SubscribeToData(player, "Foo");
        element.UnsubscribeFromAllData(player);
        element.SetData("Foo", 5, DataSyncType.Subscribe);

        server.VerifyLuaElementRpcPacketSent(ElementRpcFunction.SET_ELEMENT_DATA, player, count: 0);
    }

    [Fact]
    public void AttachElement_MovesElementToCorrectOffsetPosition()
    {
        var element = new Element() { Id = 1, Position = new(1, 2, 3) };
        var element2 = new Element() { Id = 2 };

        element2.AttachTo(element, new Vector3(1, 2, 3));

        element2.Position.Should().Be(new Vector3(2, 4, 6));
    }

    [Fact]
    public void AttachElement_MovesElementToCorrectPosition_WhenTargetIsMoved()
    {
        var element = new Element() { Id = 1, Position = new(1, 2, 3) };
        var element2 = new Element() { Id = 2 };

        element2.AttachTo(element, new Vector3(1, 2, 3));
        element.Position = new(2, 4, 6);

        element2.Position.Should().Be(new Vector3(3, 6, 9));
    }

    [Fact]
    public void DetachElement_LeavesElementInAttachedPosition()
    {
        var element = new Element() { Id = 1, Position = new(1, 2, 3) };
        var element2 = new Element() { Id = 2 };

        element2.AttachTo(element, new Vector3(1, 2, 3));
        element2.DetachFrom();

        element2.Position.Should().Be(new Vector3(2, 4, 6));
    }

    [Fact]
    public void DetachElement_DoesNotMoveElement_WhenTargetIsMoved()
    {
        var element = new Element() { Id = 1, Position = new(1, 2, 3) };
        var element2 = new Element() { Id = 2 };

        element2.AttachTo(element, new Vector3(1, 2, 3));
        element2.DetachFrom();
        element.Position = new(2, 4, 6);

        element2.Position.Should().Be(new Vector3(2, 4, 6));
    }

    [Fact]
    public void AttachedElementModifyOffset_MovesToCorrectPosition()
    {
        var element = new Element() { Id = 1, Position = new(1, 2, 3) };
        var element2 = new Element() { Id = 2 };

        var attachment = element2.AttachTo(element, new Vector3(1, 2, 3));
        attachment.PositionOffset = new(2, 3, 4);

        element2.Position.Should().Be(new Vector3(3, 5, 7));
    }

    [Fact]
    public void AttachedElementModifyOffset_TriggersEvent()
    {
        var element = new Element() { Id = 1, Position = new(1, 2, 3) };
        var element2 = new Element() { Id = 2 };

        var eventCount = 0;
        element2.AttachedOffsetChanged += (s, e) => eventCount++;

        var attachment = element2.AttachTo(element, new Vector3(1, 2, 3));
        attachment.PositionOffset = new(2, 3, 4);

        eventCount.Should().Be(1);
    }

    [Fact]
    public void AttachedElementModifyOffset_AndReattach_TriggersEventOnce()
    {
        var element = new Element() { Id = 1, Position = new(1, 2, 3) };
        var element2 = new Element() { Id = 2 };

        var eventCount = 0;
        element2.AttachedOffsetChanged += (s, e) => eventCount++;

        element2.AttachTo(element, new Vector3(1, 2, 3));
        element2.DetachFrom();
        var attachment = element2.AttachTo(element, new Vector3(1, 2, 3));
        attachment.PositionOffset = new(2, 3, 4);

        eventCount.Should().Be(1);
    }

    [Fact]
    public void AttachedElementModifyOffset_AndReattach_DoesNotTriggersEventOnOriginalAttachment()
    {
        var element = new Element() { Id = 1, Position = new(1, 2, 3) };
        var element2 = new Element() { Id = 2 };

        var eventCount = 0;
        element2.AttachedOffsetChanged += (s, e) => eventCount++;

        var attachment = element2.AttachTo(element, new Vector3(1, 2, 3));
        element2.DetachFrom();
        element2.AttachTo(element, new Vector3(1, 2, 3));
        attachment.PositionOffset = new(2, 3, 4);

        eventCount.Should().Be(0);
    }
}
