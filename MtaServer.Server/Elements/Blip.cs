using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements
{
    public class Blip : Element
    {
        public override ElementType ElementType => ElementType.Blip;

        public short Ordering { get; set; }
        public ushort VisibleDistance { get; set; }
        public BlipIcon Icon { get; set; } 
        public byte Size { get; set; } = 1;
        public Color Color { get; set; } = Color.White;

        public Blip(Vector3 position, BlipIcon icon, ushort visibleDistance = 0, short ordering = 0)
        {
            this.Position = position;
            this.Icon = icon;
            this.VisibleDistance = visibleDistance;
            this.Ordering = ordering;
        }

        public new Blip AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
