using FluentAssertions;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.TestTools;
using System.Numerics;
using Xunit;

namespace SlipeServer.Server.Tests.Integration.Behaviours;

public class CollisionShapeBehaviourTest
{
    [Fact]
    public void ColShapeEnterCalledWhenInColShape()
    {
        var server = new TestingServer();
        var behaviour = server.Instantiate<CollisionShapeBehaviour>();

        var collisionShape = new CollisionSphere(new Vector3(100, 100, 100), 10).AssociateWith(server); ;
        var dummy = new DummyElement().AssociateWith(server);

        var isEventCalled = false;
        collisionShape.ElementEntered += (_, args) =>
        {
            if (args.Element == dummy)
            {
                isEventCalled = true;
            }
        };

        dummy.Position = new Vector3(95, 100, 100);

        isEventCalled.Should().BeTrue();
    }

    [Fact]
    public void ColShapeEnterCalledOnceForTwoUpdatesInColshape()
    {
        var server = new TestingServer();
        var behaviour = server.Instantiate<CollisionShapeBehaviour>();

        var collisionShape = new CollisionSphere(new Vector3(100, 100, 100), 10).AssociateWith(server); ;
        var dummy = new DummyElement().AssociateWith(server);

        int callCount = 0;
        collisionShape.ElementEntered += (_, args) =>
        {
            if (args.Element == dummy)
            {
                callCount++;
            }
        };

        dummy.Position = new Vector3(95, 100, 100);
        dummy.Position = new Vector3(97.5f, 100, 100);

        callCount.Should().Be(1);
    }

    [Fact]
    public void ColShapeLeftCalledWhenLeaving()
    {
        var server = new TestingServer();
        var behaviour = server.Instantiate<CollisionShapeBehaviour>();

        var collisionShape = new CollisionSphere(new Vector3(100, 100, 100), 10).AssociateWith(server); ;
        var dummy = new DummyElement().AssociateWith(server);

        var isEventCalled = false;
        collisionShape.ElementLeft += (_, args) =>
        {
            if (args.Element == dummy)
            {
                isEventCalled = true;
            }
        };

        dummy.Position = new Vector3(95, 100, 100);
        dummy.Position = new Vector3(0, 100, 100);

        isEventCalled.Should().BeTrue();
    }

    [Fact]
    public void ColShapeEnterNotCalledWhenOutOfColShape()
    {
        var server = new TestingServer();
        var behaviour = server.Instantiate<CollisionShapeBehaviour>();

        var collisionShape = new CollisionSphere(new Vector3(100, 100, 100), 10).AssociateWith(server); ;
        var dummy = new DummyElement().AssociateWith(server);

        var isEventCalled = false;
        collisionShape.ElementEntered += (_, args) =>
        {
            if (args.Element == dummy)
            {
                isEventCalled = true;
            }
        };

        dummy.Position = new Vector3(0, 100, 100);

        isEventCalled.Should().BeFalse();
    }

    [Fact]
    public void ColShapeLeftNotCalledWhenNeverIn()
    {
        var server = new TestingServer();
        var behaviour = server.Instantiate<CollisionShapeBehaviour>();

        var collisionShape = new CollisionSphere(new Vector3(100, 100, 100), 10).AssociateWith(server); ;
        var dummy = new DummyElement().AssociateWith(server);

        var isEventCalled = false;
        collisionShape.ElementLeft += (_, args) =>
        {
            if (args.Element == dummy)
            {
                isEventCalled = true;
            }
        };

        dummy.Position = new Vector3(50, 100, 100);
        dummy.Position = new Vector3(0, 100, 100);

        isEventCalled.Should().BeFalse();
    }

    [Fact]
    public void ColShapeWhenInteriorChanged()
    {
        var server = new TestingServer();
        var behaviour = server.Instantiate<CollisionShapeBehaviour>();

        var collisionShape = new CollisionSphere(new Vector3(100, 100, 100), 10).AssociateWith(server);
        var dummy = new DummyElement().AssociateWith(server);

        var isEventCalled = false;
        collisionShape.ElementEntered += (_, args) =>
        {
            if (args.Element == dummy)
            {
                isEventCalled = true;
            }
        };

        dummy.Interior = 42;
        dummy.Position = new Vector3(100, 100, 100);
        isEventCalled.Should().BeFalse();
        dummy.Interior = collisionShape.Interior;
        isEventCalled.Should().BeTrue();
    }

    [Fact]
    public void ColShapeWhenPlayerSpawned()
    {
        var server = new TestingServer();
        var behaviour = server.Instantiate<CollisionShapeBehaviour>();
        var player = server.AddFakePlayer();

        var collisionShape = new CollisionSphere(new Vector3(100, 100, 100), 10).AssociateWith(server);

        var isEventCalled = false;
        collisionShape.ElementEntered += (_, args) =>
        {
            if (args.Element == player)
            {
                isEventCalled = true;
            }
        };

        isEventCalled.Should().BeFalse();
        player.Spawn(new Vector3(100, 100, 100), 0, 0, 0, 0);
        isEventCalled.Should().BeTrue();
    }
}
