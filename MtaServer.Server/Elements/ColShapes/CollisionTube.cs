using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements.ColShapes
{
    public class CollisionTube : CollisionShape
    {
        public float Radius { get; set; }
        public float Height { get; set; }

        public CollisionTube(Vector3 position, float Radius, float Height)
        {
            this.Position = position;
            this.Radius = Radius;
            this.Height = Height;
        }

        public override bool IsWithin(Vector3 position)
        {
            return Vector3.Distance(this.Position, new Vector3(position.X, position.Y, 0)) < this.Radius &&
                position.Z > this.Position.Z && position.Z < this.Position.Z + this.Height;
        }

        public new CollisionTube AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
