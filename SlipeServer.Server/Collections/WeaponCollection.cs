using SlipeServer.Server.Collections.Events;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Structs;
using SlipeServer.Server.Enums;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SlipeServer.Server.Collections;

public class WeaponCollection : IEnumerable<Weapon>
{
    private readonly Dictionary<WeaponSlot, Weapon> weapons;

    public WeaponCollection()
    {
        this.weapons = new Dictionary<WeaponSlot, Weapon>();
    }

    public void Add(Weapon weapon)
    {
        this.weapons[weapon.Slot] = weapon;
        this.WeaponAdded?.Invoke(this, weapon);

        weapon.AmmoUpdated += AmmoUpdated;
        weapon.AmmoInClipUpdated += AmmoInClipUpdated;
    }

    public void Remove(Weapon weapon)
    {
        Remove(weapon.Slot);
    }

    public void Remove(WeaponSlot slot)
    {
        if (this.weapons.TryGetValue(slot, out var weapon))
        {
            this.weapons.Remove(slot);
            this.WeaponRemoved?.Invoke(this, weapon);
        }
    }

    public void Remove(WeaponId type)
    {
        Remove(WeaponConstants.SlotPerWeapon[type]);
    }

    public Weapon? Get(WeaponSlot slot)
    {
        this.weapons.TryGetValue(slot, out var weapon);
        return weapon;
    }

    public void Clear(bool triggersUpdates = true)
    {
        if (triggersUpdates)
            foreach (var weapon in this)
                Remove(weapon);
        else
            this.weapons.Clear();
    }

    private void AmmoInClipUpdated(object? sender, AmmoUpdateEventArgs e) => this.WeaponAmmoUpdated?.Invoke(this, e.Weapon);
    private void AmmoUpdated(object? sender, AmmoUpdateEventArgs e) => this.WeaponAmmoUpdated?.Invoke(this, e.Weapon);

    public IEnumerator<Weapon> GetEnumerator() => this.weapons.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.weapons.Values.GetEnumerator();

    public event EventHandler<Weapon>? WeaponAdded;
    public event EventHandler<Weapon>? WeaponRemoved;
    public event EventHandler<Weapon>? WeaponAmmoUpdated;
}
