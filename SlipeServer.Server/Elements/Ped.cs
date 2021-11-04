using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Collections;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Elements.Structs;
using SlipeServer.Server.Enums;
using System;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.Elements
{
    public class Ped : Element
    {
        public override ElementType ElementType => ElementType.Ped;

        protected ushort model;
        public ushort Model
        {
            get => this.model;
            set
            {
                var args = new ElementChangedEventArgs<Ped, ushort>(this, this.Model, value, this.IsSync);
                this.model = value;
                ModelChanged?.Invoke(this, args);
            }
        }

        protected float health = 100;
        public float Health
        {
            get => this.health;
            set
            {
                var args = new ElementChangedEventArgs<Ped, float>(this, this.Health, value, this.IsSync);
                this.health = value;
                HealthChanged?.Invoke(this, args);
            }
        }

        protected float armor = 0;
        public float Armor
        {
            get => this.armor;
            set
            {
                var args = new ElementChangedEventArgs<Ped, float>(this, this.Armor, value, this.IsSync);
                this.armor = value;
                ArmourChanged?.Invoke(this, args);
            }
        }

        private WeaponSlot currentWeaponSlot;
        public WeaponSlot CurrentWeaponSlot
        {
            get => this.currentWeaponSlot;
            set
            {
                lock (this.CurrentWeaponLock)
                {
                    var args = new ElementChangedEventArgs<Ped, WeaponSlot>(this, this.CurrentWeaponSlot, value, this.IsSync);
                    this.currentWeaponSlot = value;
                    WeaponSlotChanged?.Invoke(this, args);
                }
            }
        }


        public object CurrentWeaponLock { get; } = new();
        public Weapon? CurrentWeapon
        {
            get => this.Weapons.Get(this.CurrentWeaponSlot);
            set
            {
                lock (this.CurrentWeaponLock)
                {
                    if (value == null)
                    {
                        this.currentWeaponSlot = WeaponSlot.Hand;
                    } else
                    {
                        this.currentWeaponSlot = value.Slot;
                        if (!this.Weapons.Any(w => w.Type == value.Type))
                        {
                            if (this.Weapons.Any(w => w.Slot == value.Slot))
                                this.Weapons.Remove(value.Slot);

                            this.Weapons.Add(value);
                        } else
                        {
                            var weapon = this.Weapons.Get(value.Slot);
                            if (weapon != null && weapon.Ammo != value.Ammo)
                                weapon.Ammo = value.Ammo;
                            if (weapon != null && weapon.AmmoInClip != value.AmmoInClip)
                                weapon.AmmoInClip = value.AmmoInClip;

                        }
                        this.CurrentWeaponSlot = value.Slot;
                    }
                }
            }
        }

        public float PedRotation
        {
            get => this.Rotation.Z;
            set => this.Rotation = new Vector3(this.rotation.X, this.rotation.Y, value);
        }

        public Vehicle? Vehicle { get; set; }
        public byte? Seat { get; set; }

        private bool hasJetpack = false;
        public bool HasJetpack
        {
            get => this.hasJetpack;
            set
            {
                var args = new ElementChangedEventArgs<Ped, bool>(this, this.hasJetpack, value, this.IsSync);
                this.hasJetpack = value;
                JetpackStateChanged?.Invoke(this, args);
            }
        }
        public bool IsSyncable { get; set; } = true;
        public bool IsHeadless { get; set; } = false;
        public bool IsFrozen { get; set; } = false;
        public PedMoveAnimation MoveAnimation { get; set; } = 0;
        public PedClothing[] Clothes { get; set; }
        public WeaponCollection Weapons { get; set; }
        public bool IsAlive => this.health > 0;
        public Player? Syncer { get; set; }
        public bool IsOnFire { get; set; }
        public bool IsInWater { get; set; }

        private Element? target = null;
        public Element? Target
        {
            get => this.target;
            set
            {
                var args = new ElementChangedEventArgs<Ped, Element?>(this, this.Target, value, this.IsSync);
                this.target = value;
                TargetChanged?.Invoke(this, args);
            }
        }
        public VehicleAction VehicleAction { get; set; } = VehicleAction.None;
        public Vehicle? JackingVehicle { get; set; }


        public Ped(PedModel model, Vector3 position) : base()
        {
            this.Model = (ushort)model;
            this.Position = position;

            this.Clothes = Array.Empty<PedClothing>();
            this.Weapons = new WeaponCollection();
            this.Weapons.WeaponAdded += (sender, args) => this.WeaponReceived?.Invoke(this, new WeaponReceivedEventArgs(this, args.Type, args.Ammo, false));
            this.Weapons.WeaponRemoved += (sender, args) => this.WeaponRemoved?.Invoke(this, new WeaponRemovedEventArgs(this, args.Type, args.Ammo));
            this.Weapons.WeaponAmmoUpdated += (sender, args) => this.AmmoUpdated?.Invoke(this, new AmmoUpdateEventArgs(this, args.Type, args.Ammo, args.AmmoInClip));
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

        public void AddWeapon(WeaponId weaponId, ushort ammoCount, bool setAsCurrent = false)
        {
            this.Weapons.Add(new Weapon(weaponId, ammoCount, null));
            this.WeaponReceived?.Invoke(this, new WeaponReceivedEventArgs(this, weaponId, ammoCount, setAsCurrent));
        }

        public void RemoveWeapon(WeaponId weaponId, ushort? ammoCount = null)
        {
            var weapon = this.Weapons.FirstOrDefault(weapon => weapon.Type == weaponId);
            if (weapon != null)
            {
                this.Weapons.Remove(weapon);
                this.WeaponRemoved?.Invoke(this, new WeaponRemovedEventArgs(this, weaponId, ammoCount));
            }
        }

        public void RemoveWeapon(WeaponSlot weaponSlot, ushort? ammoCount = null)
        {
            var weapon = this.Weapons.FirstOrDefault(weapon => weapon.Slot == weaponSlot);
            if (weapon != null)
                RemoveWeapon(weapon.Type, ammoCount);
        }

        public void SetAmmoCount(WeaponId weaponId, ushort count, ushort? inClip)
        {
            var weapon = this.Weapons.FirstOrDefault(weapon => weapon.Type == weaponId);
            if (weapon != null)
            {
                weapon.Ammo = count;
                if (inClip != null)
                {
                    weapon.AmmoInClip = Math.Min(inClip.Value, count);
                }
                this.AmmoUpdated?.Invoke(this, new AmmoUpdateEventArgs(this, weaponId, count, inClip));
            }
        }

        public void SetAmmoCount(WeaponSlot weaponSlot, ushort count, ushort? inClip)
        {
            var weapon = this.Weapons.FirstOrDefault(weapon => weapon.Slot == weaponSlot);
            if (weapon != null)
                SetAmmoCount(weapon.Type, count, inClip);
        }

        protected void InvokeWasted(PedWastedEventArgs args) => this.Wasted?.Invoke(this, args);

        public virtual void Kill(Element? damager, WeaponType damageType, BodyPart bodyPart, ulong animationGroup = 0, ulong animationId = 15)
        {
            this.RunAsSync(() =>
            {
                this.health = 0;
                this.Vehicle = null;
                this.Seat = null;
                this.VehicleAction = VehicleAction.None;
                InvokeWasted(new PedWastedEventArgs(this, damager, damageType, bodyPart, animationGroup, animationId));
            });
        }

        public void Kill(WeaponType damageType = WeaponType.WEAPONTYPE_UNARMED, BodyPart bodyPart = BodyPart.Torso)
        {
            this.Kill(null, damageType, bodyPart);
        }

        public event ElementEventHandler<Ped, PedWastedEventArgs>? Wasted;
        public event ElementChangedEventHandler<Ped, ushort>? ModelChanged;
        public event ElementChangedEventHandler<Ped, float>? HealthChanged;
        public event ElementChangedEventHandler<Ped, float>? ArmourChanged;
        public event ElementChangedEventHandler<Ped, WeaponSlot>? WeaponSlotChanged;
        public event ElementChangedEventHandler<Ped, bool>? JetpackStateChanged;
        public event ElementChangedEventHandler<Ped, Element?>? TargetChanged;
        public event ElementEventHandler<Ped, WeaponReceivedEventArgs>? WeaponReceived;
        public event ElementEventHandler<Ped, WeaponRemovedEventArgs>? WeaponRemoved;
        public event ElementEventHandler<Ped, AmmoUpdateEventArgs>? AmmoUpdated;
    }
}
