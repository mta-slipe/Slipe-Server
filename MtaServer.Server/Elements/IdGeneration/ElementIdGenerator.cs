using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements.IdGeneration
{
    public class ElementIdGenerator: IElementIdGenerator
    {
        private uint idCounter;

        public ElementIdGenerator()
        {
            this.idCounter = 0;
        }

        public uint GetId()
        {
            return ++idCounter;
        }
    }
}
