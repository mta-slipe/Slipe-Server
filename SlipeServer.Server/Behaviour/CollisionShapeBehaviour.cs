using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
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

        public CollisionShapeBehaviour(MtaServer server, IElementRepository elementRepository)
        {
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
            } else
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
