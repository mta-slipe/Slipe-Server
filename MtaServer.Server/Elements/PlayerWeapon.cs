using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements
{
    public struct PlayerWeapon
    {
        public byte WeaponType { get; set; }
        public byte Slot { get; set; }
        public ushort Ammo { get; set; }
        public ushort AmmoInClip { get; set; }
    }
}
