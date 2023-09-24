using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedWeaponOrAmmoChangedEventArgs : EventArgs
{
    public Ped Ped { get; }
    public WeaponId Weapon { get; }
    public ushort Ammo { get; }
    public ushort AmmoInClip { get; }

    public PedWeaponOrAmmoChangedEventArgs(Ped ped, WeaponId weapon, ushort ammo, ushort ammoInClip)
    {
        this.Ped = ped;
        this.Weapon = weapon;
        this.Ammo = ammo;
        this.AmmoInClip = ammoInClip;
    }
}
