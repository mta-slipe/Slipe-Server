using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Entities.Structs
{
    public struct PedClothing
    {
        public string Texture { get; set; }
        public string Model { get; set; }
        public byte Type { get; set; }

    }
}
