using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Entities.Structs
{
    public struct CustomDataItem
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }
}
