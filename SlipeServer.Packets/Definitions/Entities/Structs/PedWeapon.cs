using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Entities.Structs
{
    public struct PedWeapon
    {
        public byte Slot { get; set; }
        public byte Type { get; set; }
        public ushort Ammo { get; set; }
    }
}
