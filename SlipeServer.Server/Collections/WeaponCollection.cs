using SlipeServer.Server.Collections.Events;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Structs;
using SlipeServer.Server.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipeServer.Server.Collections
{
    public class WeaponCollection: IEnumerable<Weapon>
    {
        private readonly List<Weapon> weapons;

        public WeaponCollection()
        {
            this.weapons = new List<Weapon>();
        }

        public void Add(Weapon weapon)
        {
            this.weapons.Add(weapon);
            this.WeaponAdded?.Invoke(this, weapon);

            weapon.AmmoUpdated += AmmoUpdated;
            weapon.AmmoInClipUpdated += AmmoInClipUpdated;
        }

        public void Remove(Weapon weapon)
        {
            this.weapons.Remove(weapon);
            this.WeaponRemoved?.Invoke(this, weapon);
        }

        public void Remove(WeaponSlot slot)
        {
            var weapon = this.weapons.FirstOrDefault(weapon => weapon.Slot == slot);
            if (weapon != null)
                Remove(weapon);
        }

        public void Remove(WeaponId type)
        {
            var weapon = this.weapons.FirstOrDefault(weapon => weapon.Type == type);
            if (weapon != null)
                Remove(weapon);
        }

        private void AmmoInClipUpdated(object? sender, AmmoUpdateEventArgs e) => this.WeaponAmmoUpdated?.Invoke(this, e.Weapon);
        private void AmmoUpdated(object? sender, AmmoUpdateEventArgs e) => this.WeaponAmmoUpdated?.Invoke(this, e.Weapon);

        public IEnumerator<Weapon> GetEnumerator() => this.weapons.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.weapons.GetEnumerator();

        public event EventHandler<Weapon>? WeaponAdded;
        public event EventHandler<Weapon>? WeaponRemoved;
        public event EventHandler<Weapon>? WeaponAmmoUpdated;
    }
}
