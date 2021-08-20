using SlipeServer.Server.Elements.Events;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Server.Elements
{
    public class RadarArea : Element
    {
        public override ElementType ElementType => ElementType.RadarArea;
        private Color color;
        private Vector2 size;
        public Vector2 Position2
        {
            get => new(this.Position.X, this.Position.Y);
            set => this.Position = new Vector3(value.X, value.Y, 0);
        }

        public Vector2 Size
        {
            get => this.size; set
            {
                var args = new ElementChangedEventArgs<RadarArea, Vector2>(this, this.size, value, this.IsSync);
                this.size = value;
                SizeChanged?.Invoke(this, args);
            }
        }

        public Color Color
        {
            get => this.color; set
            {
                var args = new ElementChangedEventArgs<RadarArea, Color>(this, this.color, value, this.IsSync);
                this.color = value;
                ColorChanged?.Invoke(this, args);
            }
        }
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

        public event ElementChangedEventHandler<RadarArea, Vector2>? SizeChanged;
        public event ElementChangedEventHandler<RadarArea, Color>? ColorChanged;
    }
}
