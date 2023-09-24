using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for triggering collision shape enter and exit events when an element's position changes
/// </summary>
public class CollisionShapeBehaviour
{
    private readonly ConcurrentDictionary<CollisionShape, byte> collisionShapes;

    public CollisionShapeBehaviour(MtaServer server, IElementCollection elementCollection)
    {
        this.collisionShapes = new();
        foreach (var collisionShape in elementCollection.GetByType<CollisionShape>(ElementType.Colshape))
        {
            this.AddCollisionShape(collisionShape);
        }

        server.ElementCreated += OnElementCreate;
    }

    private void OnElementCreate(Element element)
    {
        if (element is CollisionShape collisionShape)
            AddCollisionShape(collisionShape);
        else if(element is Player player)
        {
            player.Spawned += OnPlayerSpawned;
            player.Destroyed += OnPlayerDestroyed;
        }
        else
            element.PositionChanged += OnElementPositionChange;
    }

    private void OnPlayerDestroyed(Element element)
    {
        var player = (Player)element;
        player.Spawned -= OnPlayerSpawned;
        player.Destroyed -= OnPlayerDestroyed;
    }

    private void OnPlayerSpawned(Player sender, PlayerSpawnedEventArgs e)
    {
        RefreshColliders(sender);
    }

    private void AddCollisionShape(CollisionShape collisionShape)
    {
        this.collisionShapes[collisionShape] = 0;
        collisionShape.Destroyed += RemoveCollisionShape;
    }

    private void RemoveCollisionShape(Element collisionShape)
    {
        this.collisionShapes.Remove((CollisionShape)collisionShape, out var _);
    }

    private void RefreshColliders(Element element)
    {
        foreach (var shape in this.collisionShapes)
        {
            shape.Key.CheckElementWithin(element);
        }
    }

    private void OnElementPositionChange(object sender, ElementChangedEventArgs<Vector3> eventArgs)
    {
        eventArgs.Source.RunWithContext(
            () => RefreshColliders(eventArgs.Source),
            Elements.Enums.ElementUpdateContext.PostEvent
        );
    }
}
