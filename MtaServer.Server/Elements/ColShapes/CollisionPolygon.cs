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

        public override bool IsWithin(Vector3 position)
        {
            Vector2 point = new Vector2(position.X, position.Y);

            uint intersections = 0;
            for (int i = 0; i < this.Vertices.Length; i++)
            {
                var pointA = this.Vertices[i];
                var pointB = this.Vertices[(i + 1 == this.Vertices.Length) ? 0 : (i + 1)];

                if (DoLinesIntersect(pointA, pointB, point - new Vector2(float.MaxValue, 0) ,point))
                    intersections++;
            }

            return intersections % 2 == 1;
        }

        public new CollisionPolygon AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        private bool IsOnSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            return (
                q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y)
            );
        }

        private int GetOrientation(Vector2 p, Vector2 q, Vector2 r)
        {
            float val = (q.Y - p.Y) * (r.X - q.X) -
                    (q.X - p.X) * (r.Y - q.Y);

            return 
                val == 0 ? 0 :
                val > 0 ? 1 :
                2;
        }

        private bool DoLinesIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            int o1 = GetOrientation(p1, q1, p2);
            int o2 = GetOrientation(p1, q1, q2);
            int o3 = GetOrientation(p2, q2, p1);
            int o4 = GetOrientation(p2, q2, q1);

            return (o1 != o2 && o3 != o4) ||
                (o1 == 0 && IsOnSegment(p1, p2, q1)) ||
                (o2 == 0 && IsOnSegment(p1, q2, q1)) ||
                (o3 == 0 && IsOnSegment(p2, p1, q2)) ||
                (o4 == 0 && IsOnSegment(p2, q1, q2));
        }
    }
}
