using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Behaviour
{
    /// <summary>
    /// Behaviour responsible for triggering collision shape enter and exit events when an element's position changes.
    /// This allows for only handling position changes for specific types of elements.
    /// </summary>
    public class TypeFilteredCollisionShapeBehaviour
    {
        private readonly HashSet<CollisionShape> collisionShapes;
        private readonly HashSet<Type> types;

        public TypeFilteredCollisionShapeBehaviour(MtaServer server, IElementRepository elementRepository, IEnumerable<Type> types)
        {
            this.types = new HashSet<Type>(types);

            this.collisionShapes = new HashSet<CollisionShape>();
            foreach (var collisionShape in elementRepository.GetByType<CollisionShape>(ElementType.Colshape))
            {
                this.AddCollisionShape(collisionShape);
            }

            server.ElementCreated += OnElementCreate;
        }

        private void OnElementCreate(Element element)
        {
            if (element is CollisionShape collisionShape)
            {
                AddCollisionShape(collisionShape);
            } else if (this.types.Contains(element.GetType()))
            {
                element.PositionChanged += OnElementPositionChange;
            }
        }

        private void AddCollisionShape(CollisionShape collisionShape)
        {
            this.collisionShapes.Add(collisionShape);
            collisionShape.Destroyed += (source) => this.collisionShapes.Remove(collisionShape);
        }

        private void OnElementPositionChange(object sender, ElementChangedEventArgs<Vector3> eventArgs)
        {
            foreach (var shape in this.collisionShapes)
            {
                shape.CheckElementWithin(eventArgs.Source);
            }
        }
    }
}
