using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements
{
    public static class ElementIdGenerator
    {
        private static uint idCounter = 0;
        public static uint GenerateId()
        {
            idCounter++;
            return idCounter;
        }
    }
}
