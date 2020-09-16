using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements
{
    public class RadarArea : Element
    {
        public override ElementType ElementType => ElementType.RadarArea;

        public Vector2 Position2
        {
            get => new Vector2(Position.X, Position.Y);
            set
            {
                Position = new Vector3(value.X, value.Y, 0);
            }
        }

        public Vector2 Size { get; set; }
        public Color Color { get; set; }
        public bool IsFlashing { get; set; } = false;

        public RadarArea(Vector2 position, Vector2 size, Color color)
        {
            this.Position2 = position; 
            this.Size = size;
            this.Color = color;
        }

        public new RadarArea AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
