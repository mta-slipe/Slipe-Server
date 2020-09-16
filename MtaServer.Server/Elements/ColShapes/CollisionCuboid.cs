using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements.ColShapes
{
    public class CollisionCuboid : CollisionShape
    {
        public Vector3 Dimensions { get; set; }

        public CollisionCuboid(Vector3 position, Vector3 dimensions)
        {
            Position = position;
            Dimensions = dimensions;
        }

        public override bool IsWithin(Vector3 position)
        {
            Vector3 bounds = this.Position + this.Dimensions;

            return
                position.X > this.Position.X && position.X < bounds.X &&
                position.Y > this.Position.Y && position.Y < bounds.Y &&
                position.Z > this.Position.Z && position.Z < bounds.Z;
        }

        public new CollisionCuboid AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
