using MtaServer.Server.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements.IdGeneration
{
    public class BasicElementIdGenerator : IElementIdGenerator
    {
        private uint idCounter;

        public BasicElementIdGenerator()
        {
            this.idCounter = 0;
        }

        public uint GetId()
        {
            this.idCounter = (this.idCounter + 1) % ElementConstants.MaxElementId;
            return idCounter;
        }
    }
}
