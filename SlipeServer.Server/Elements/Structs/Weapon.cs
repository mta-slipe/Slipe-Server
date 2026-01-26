using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Collections.Events;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Structs;

public class Weapon(WeaponId weaponId, ushort ammo, ushort? ammoInClip = null)
{
    public WeaponSlot Slot { get; } = WeaponConstants.SlotPerWeapon[weaponId];
    public WeaponId Type { get; } = weaponId;

    private ushort ammo = ammo;
    public ushort Ammo
    {
        get => this.ammo;
        set
        {
            if (value == this.ammo)
                return;

            this.ammo = value;
            this.AmmoUpdated?.Invoke(this, new AmmoUpdateEventArgs(this, this.ammo, this.ammoInClip));
        }
    }

    private ushort ammoInClip = ammoInClip ?? WeaponConstants.ClipCountsPerWeapon[weaponId];
    public ushort AmmoInClip
    {
        get => this.ammoInClip;
        set
        {
            if (value == this.ammoInClip)
                return;

            this.ammoInClip = value;
            this.AmmoInClipUpdated?.Invoke(this, new AmmoUpdateEventArgs(this, this.ammo, this.ammoInClip));
        }
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
