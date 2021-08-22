﻿using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Behaviour
{
    /// <summary>
    /// Behaviour responsible for triggering collision shape enter and exit events when an element's position changes
    /// </summary>
    public class CollisionShapeBehaviour
    {
        private readonly HashSet<CollisionShape> collisionShapes;
        private readonly MtaServer server;

        public CollisionShapeBehaviour(MtaServer server, IElementRepository elementRepository)
        {
            this.collisionShapes = new HashSet<CollisionShape>();
            foreach (var collisionShape in elementRepository.GetByType<CollisionShape>(ElementType.Colshape))
            {
                this.AddCollisionShape(collisionShape);
            }

            server.ElementCreated += OnElementCreate;
            this.server = server;
        }

        private void OnElementCreate(Element element)
        {
            if (element is CollisionShape collisionShape)
            {
                AddCollisionShape(collisionShape);
                if (collisionShape is CollisionCircle collisionCircle)
                {
                    collisionCircle.RadiusChanged += HandleRadiusChange;
                }
                else if (collisionShape is CollisionSphere collisionSphere)
                {
                    collisionSphere.RadiusChanged += HandleRadiusChange;
                }
                else if (collisionShape is CollisionTube collisionTube)
                {
                    collisionTube.RadiusChanged += HandleRadiusChange;
                }
                else if (collisionShape is CollisionPolygon collisionPolygon)
                {
                    collisionPolygon.HeightChanged += HandleHeightChanged;
                }
            } else
            {
                element.PositionChanged += OnElementPositionChange;
            }
        }

        private void HandleHeightChanged(Element sender, ElementChangedEventArgs<Vector2> args)
        {
            this.server.BroadcastPacket(CollisionShapePacketFactory.CreateSetHeight(args.Source, args.NewValue));
        }

        private void HandleRadiusChange(Element sender, ElementChangedEventArgs<float> args)
        {
            this.server.BroadcastPacket(CollisionShapePacketFactory.CreateSetRadius(args.Source, args.NewValue));
        }

        private void AddCollisionShape(CollisionShape collisionShape)
        {
            this.collisionShapes.Add(collisionShape);
            collisionShape.Destroyed += (source) => this.collisionShapes.Remove(collisionShape);
        }

        private void RefreshColliders(Element element)
        {
            foreach (var shape in this.collisionShapes)
            {
                shape.CheckElementWithin(element);
            }
        }

        private void OnElementPositionChange(object sender, ElementChangedEventArgs<Vector3> eventArgs)
        {
            RefreshColliders(eventArgs.Source);
        }
    }
}
