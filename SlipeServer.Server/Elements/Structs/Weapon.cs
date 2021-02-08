using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Structs
{
    public class Weapon
    {
        public WeaponSlot Slot { get; set; }
        public WeaponId Type { get; set; }
        public ushort Ammo { get; set; }
        public ushort AmmoInClip { get; set; }


        public static implicit operator PedWeapon(Weapon weapon) => new PedWeapon() { Slot = (byte)weapon.Slot, Type = (byte)weapon.Type, Ammo = weapon.Ammo, AmmoInClip = weapon.AmmoInClip };
    }


}
