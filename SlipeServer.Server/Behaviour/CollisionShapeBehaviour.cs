﻿using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
using System;
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
        else
        {
            element.PositionChanged += OnElementPositionChange;
            element.InteriorChanged += OnElementInteriorChanged;
            element.DimensionChanged += HandleElementDimensionChanged;
            if(element is Player player)
                player.Spawned += HandleSpawned;
        }
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

    private void OnElementInteriorChanged(Element sender, ElementChangedEventArgs<byte> eventArgs)
    {
        eventArgs.Source.RunWithContext(
            () => RefreshColliders(eventArgs.Source),
            Elements.Enums.ElementUpdateContext.PostEvent
        );
    }

    private void HandleElementDimensionChanged(Element sender, ElementChangedEventArgs<ushort> eventArgs)
    {
        eventArgs.Source.RunWithContext(
            () => RefreshColliders(eventArgs.Source),
            Elements.Enums.ElementUpdateContext.PostEvent
        );
    }

    private void HandleSpawned(Player sender, PlayerSpawnedEventArgs eventArgs)
    {
        eventArgs.Source.RunWithContext(
            () => RefreshColliders(eventArgs.Source),
            Elements.Enums.ElementUpdateContext.PostEvent
        );
    }
}
