using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements.ColShapes
{
    public class CollisionSphere : CollisionShape
    {
        public float Radius { get; set; }

        public CollisionSphere(Vector3 position, float Radius)
        {
            this.Position = position;
            this.Radius = Radius;
        }

        public override bool IsWithin(Vector3 position)
        {
            return Vector3.Distance(this.Position, position) < this.Radius;
        }

        public new CollisionSphere AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
