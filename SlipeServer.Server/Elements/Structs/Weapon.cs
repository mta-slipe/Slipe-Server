using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Collections.Events;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Structs;

public class Weapon
{
    public WeaponSlot Slot { get; }
    public WeaponId Type { get; }

    private ushort ammo;
    public ushort Ammo
    {
        get => this.ammo;
        set
        {
            this.ammo = value;
            this.AmmoUpdated?.Invoke(this, new AmmoUpdateEventArgs(this, this.ammo, this.ammoInClip));
        }
    }

    private ushort ammoInClip;
    public ushort AmmoInClip
    {
        get => this.ammoInClip;
        set
        {
            this.ammoInClip = value;
            this.AmmoInClipUpdated?.Invoke(this, new AmmoUpdateEventArgs(this, this.ammo, this.ammoInClip));
        }
    }

    public Weapon(WeaponId weaponId, ushort ammo, ushort? ammoInClip = null)
    {
        this.Type = weaponId;
        this.Slot = WeaponConstants.SlotPerWeapon[weaponId];
        this.ammo = ammo;
        this.ammoInClip = ammoInClip ?? WeaponConstants.ClipCountsPerWeapon[weaponId];
    }

    public void UpdateAmmoCountWithoutTriggerEvent(ushort ammo, ushort? ammoInClip)
    {
        this.ammo = ammo;
        if (ammoInClip != null)
            this.ammoInClip = ammoInClip.Value;
    }

    public static implicit operator PedWeapon(Weapon weapon) => new() { Slot = (byte)weapon.Slot, Type = (byte)weapon.Type, Ammo = weapon.Ammo, AmmoInClip = weapon.AmmoInClip };

    public event EventHandler<AmmoUpdateEventArgs>? AmmoUpdated;
    public event EventHandler<AmmoUpdateEventArgs>? AmmoInClipUpdated;
}
