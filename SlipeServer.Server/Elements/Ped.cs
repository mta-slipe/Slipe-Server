using System;
using SlipeServer.Packets;
using System.Net;
using System.Numerics;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Elements.Structs;
using SlipeServer.Packets.Definitions.Entities.Structs;
using System.Collections.Generic;
using SlipeServer.Server.Constants;
using System.Linq;

namespace SlipeServer.Server.Elements
{
    public class Ped: Element
    {
        public override ElementType ElementType => ElementType.Ped;

        protected ushort model;
        public ushort Model
        {
            get => model;
            set
            {
                var args = new ElementChangedEventArgs<Ped, ushort>(this, this.Model, value, this.IsSync);
                model = value;
                ModelChanged?.Invoke(this, args);
            }
        }

        protected float health = 100;
        public float Health
        {
            get => health;
            set
            {
                var args = new ElementChangedEventArgs<Ped, float>(this, this.Health, value, this.IsSync);
                health = value;
                HealthChanged?.Invoke(this, args);
            }
        }

        protected float armor = 0;
        public float Armor
        {
            get => armor;
            set
            {
                var args = new ElementChangedEventArgs<Ped, float>(this, this.Armor, value, this.IsSync);
                armor = value;
                ArmourChanged?.Invoke(this, args);
            }
        }

        public Weapon? CurrentWeapon { get; set; }
        public float PedRotation { get; set; } = 0;
        public Vehicle? Vehicle { get; set; }
        public byte? Seat { get; set; }
        public bool HasJetpack { get; set; } = false;
        public bool IsSyncable { get; set; } = true;
        public bool IsHeadless { get; set; } = false;
        public bool IsFrozen { get; set; } = false;
        public PedMoveAnimation MoveAnimation { get; set; } = 0;
        public PedClothing[] Clothes { get; set; }
        public List<Weapon> Weapons { get; set; }

        public bool IsAlive => health > 0;


        public VehicleAction VehicleAction { get; set; } = VehicleAction.None;
        public Vehicle? JackingVehicle { get; set; }


        public Ped(ushort model, Vector3 position): base()
        {
            this.Model = model;
            this.Position = position;

            this.Clothes = new PedClothing[0];
            this.Weapons = new List<Weapon>();
        }

        public new Ped AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public void RemoveFromVehicle(bool warpOut = true)
        {
            this.Vehicle?.RemovePassenger(this, warpOut);
        }

        public void WarpIntoVehicle(Vehicle vehicle, byte seat = 0)
        {
            if (!this.IsAlive || vehicle.Health <= 0)
                return;

            if (vehicle.Driver != null && vehicle.Driver.VehicleAction != VehicleAction.None)
                return;

            vehicle.AddPassenger(seat, this, true);    
        }

        public void AddWeapon(WeaponId weaponId, ushort ammoCount, ushort inClip = 0)
        {
            this.Weapons.Add(new Weapon()
            {
                Type = weaponId,
                Ammo = ammoCount,
                AmmoInClip = inClip,
                Slot = WeaponConstants.SlotPerWeapon[weaponId]
            });
        }

        public void RemoveWeapon(WeaponId weaponId)
        {
            this.Weapons.RemoveAll(weapon => weapon.Type == weaponId);
        }

        public void SetAmmoCount(WeaponId weaponId, ushort count)
        {
            var weapon = this.Weapons.FirstOrDefault(weapon => weapon.Type == weaponId);
            if (weapon != null)
            {
                weapon.Ammo = count;
            }
        }

        public event ElementChangedEventHandler<Ped, ushort>? ModelChanged;
        public event ElementChangedEventHandler<Ped, float>? HealthChanged;
        public event ElementChangedEventHandler<Ped, float>? ArmourChanged;
    }
}
