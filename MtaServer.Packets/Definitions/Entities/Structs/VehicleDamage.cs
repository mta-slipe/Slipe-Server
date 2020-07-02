using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Entities.Structs
{
    public struct VehicleDamage
    {
        public byte[] Doors { get; set; }
        public byte[] Wheels { get; set; }
        public byte[] Panels { get; set; }
        public byte[] Lights { get; set; }
    }
}
