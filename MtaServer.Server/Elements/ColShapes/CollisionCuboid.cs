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
            this.Position = position;
            Dimensions = dimensions;
        }
    }
}
