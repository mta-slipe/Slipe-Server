using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements.ColShapes
{
    public abstract class CollisionShape : Element
    {
        public override ElementType ElementType => ElementType.Colshape;

        public bool IsEnabled { get; set; } = true;
        public bool AutoCallEvent { get; set; } = true;

        public abstract bool IsWithin(Vector3 position);

        public bool IsWithin(Element element) => IsWithin(element.Position);

        public new CollisionShape AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
