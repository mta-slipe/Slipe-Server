using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Entities.Structs
{
    public struct VehicleSirenSet
    {
        public byte SirenType { get; set; }
        public VehicleSiren[] Sirens { get; set; }
    }
}
