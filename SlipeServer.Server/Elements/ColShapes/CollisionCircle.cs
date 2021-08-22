using SlipeServer.Server.Elements.Events;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Server.Elements.ColShapes
{
    public class CollisionCircle : CollisionShape
    {
        public Vector2 Position2
        {
            get => new(this.Position.X, this.Position.Y);
            set => this.Position = new Vector3(value.X, value.Y, 0);
        }


        private float radius;
        public float Radius { get => this.radius; set
            {
                var args = new ElementChangedEventArgs<float>(this, this.radius, value, this.IsSync);
                this.radius = value;
                RadiusChanged?.Invoke(this, args);
            }
        }

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

        public event ElementChangedEventHandler<float>? RadiusChanged;
    }
}
