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
    }
}
