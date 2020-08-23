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

        public static VehicleDamage Undamaged => new VehicleDamage()
        {
            Doors = new byte[6],
            Wheels = new byte[4],
            Panels = new byte[7],
            Lights = new byte[4],
        };
    }
}
