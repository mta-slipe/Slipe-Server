using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedWeaponOrAmmoChangedEventArgs(Ped ped, WeaponId weapon, ushort ammo, ushort ammoInClip) : EventArgs
{
    public Ped Ped { get; } = ped;
    public WeaponId Weapon { get; } = weapon;
    public ushort Ammo { get; } = ammo;
    public ushort AmmoInClip { get; } = ammoInClip;
}
