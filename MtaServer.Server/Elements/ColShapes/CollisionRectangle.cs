using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements.ColShapes
{
    public class CollisionRectangle : CollisionShape
    {
        public Vector2 Position2
        {
            get => new Vector2(Position.X, Position.Y);
            set
            {
                Position = new Vector3(value.X, value.Y, 0);
            }
        }

        public Vector2 Dimensions { get; set; }

        public CollisionRectangle(Vector2 position, Vector2 dimensions)
        {
            this.Position2 = position;
            Dimensions = dimensions;
        }

        public override bool IsWithin(Vector3 position)
        {
            Vector2 bounds = this.Position2 + this.Dimensions;

            return
                position.X > this.Position.X && position.X < bounds.X &&
                position.Y > this.Position.Y && position.Y < bounds.Y;
        }

        public new CollisionRectangle AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
