using System;
using System.Numerics;

namespace MtaServer.Server.Elements
{
    public class DummyElement: Element
    {
        public override ElementType ElementType => ElementType.Dummy;
    }
}
