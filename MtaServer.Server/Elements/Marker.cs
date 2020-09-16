using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements
{
    public class Marker : Element
    {
        public override ElementType ElementType => ElementType.Marker;

        public MarkerType MarkerType { get; set; }
        public float Size { get; set; } = 1;
        public Color Color { get; set; } = Color.White;
        public Vector3? TargetPosition { get; set; }

        public Marker(Vector3 position, MarkerType markerType)
        {
            this.Position = position;
            this.MarkerType = markerType;
        }

        public new Marker AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
