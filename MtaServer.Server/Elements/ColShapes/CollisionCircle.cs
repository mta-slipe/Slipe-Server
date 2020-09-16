using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements.ColShapes
{
    public class CollisionCircle : CollisionShape
    {
        public Vector2 Position2
        {
            get => new Vector2(Position.X, Position.Y);
            set
            {
                Position = new Vector3(value.X, value.Y, 0);
            }
        }

        public float Radius { get; set; }

        public CollisionCircle(Vector2 position, float Radius)
        {
            this.Position2 = position;
            this.Radius = Radius;
        }

        public override bool IsWithin(Vector3 position)
        {
            return Vector3.Distance(this.Position, new Vector3(position.X, position.Y, 0)) < this.Radius;
        }

        public new CollisionCircle AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
