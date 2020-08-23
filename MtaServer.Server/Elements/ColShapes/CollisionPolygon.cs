using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements.ColShapes
{
    public class CollisionPolygon : CollisionShape
    {
        public Vector2[] Vertices { get; set; }

        public CollisionPolygon(Vector3 position, Vector2[] vertices)
        {
            this.Position = position;
            this.Vertices = vertices;
        }
    }
}
