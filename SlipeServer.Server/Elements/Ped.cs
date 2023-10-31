using SlipeServer.Packets.Enums;
using SlipeServer.Server.Collections;
using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Elements.Structs;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A ped element
/// A ped (short for pedestrian) is a non player character, that can be controlled by code.
/// </summary>
public class Ped : Element
{
    public override ElementType ElementType => ElementType.Ped;

    protected ushort model;
    public ushort Model
    {
        get => this.model;
        set
        {
            if (this.model == value)
                return;

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
            if (this.health == value)
                return;

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
            if (this.armor == value)
                return;

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
            if (this.currentWeaponSlot == value)
                return;

            lock (this.CurrentWeaponLock)
            {
                var args = new ElementChangedEventArgs<Ped, WeaponSlot>(this, this.CurrentWeaponSlot, value, this.IsSync);
                this.currentWeaponSlot = value;
                WeaponSlotChanged?.Invoke(this, args);
            }
        }
    }

    private FightingStyle fightingStyle = FightingStyle.Standard;
    public FightingStyle FightingStyle
    {
        get => this.fightingStyle;
        set
        {
            if (this.fightingStyle == value)
                return;

            var args = new ElementChangedEventArgs<Ped, FightingStyle>(this, this.fightingStyle, value, this.IsSync);
            this.fightingStyle = value;
            FightingStyleChanged?.Invoke(this, args);
        }
    }

    private float gravity = GravityConstants.Default;
    public float Gravity
    {
        get => this.gravity;
        set
        {
            if (this.gravity == value)
                return;

            var args = new ElementChangedEventArgs<Ped, float>(this, this.gravity, value, this.IsSync);
            this.gravity = value;
            GravityChanged?.Invoke(this, args);
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

    private Vehicle? enteringVehicle;
    public Vehicle? EnteringVehicle
    {
        get => this.enteringVehicle;
        set
        {
            if (this.enteringVehicle == value)
                return;

            var args = new ElementChangedEventArgs<Ped, Vehicle?>(this, this.vehicle, value, this.IsSync);
            if(this.enteringVehicle != null)
                this.enteringVehicle.Destroyed -= HandleEnteringVehicleDestroyed;
            this.enteringVehicle = value;
            if(value != null)
                value.Destroyed += HandleEnteringVehicleDestroyed;
            EnteringVehicleChanged?.Invoke(this, args);
        }
    }

    private void HandleEnteringVehicleDestroyed(Element destroyedVehicle)
    {
        this.VehicleAction = VehicleAction.None;
        this.EnteringVehicle = null;
    }


    private Vehicle? vehicle;
    public Vehicle? Vehicle
    {
        get => this.vehicle;
        set
        {
            if (this.vehicle == value)
                return;

            var args = new ElementChangedEventArgs<Ped, Vehicle?>(this, this.vehicle, value, this.IsSync);
            this.vehicle = value;
            VehicleChanged?.Invoke(this, args);
        }
    }

    public byte? Seat { get; set; }

    private bool hasJetpack = false;
    public bool HasJetpack
    {
        get => this.hasJetpack;
        set
        {
            if (this.hasJetpack == value)
                return;

            if (this.Vehicle != null)
                return;

            var args = new ElementChangedEventArgs<Ped, bool>(this, this.hasJetpack, value, this.IsSync);
            this.hasJetpack = value;
            JetpackStateChanged?.Invoke(this, args);
        }
    }
    public bool IsSyncable { get; set; } = true;
    public bool IsHeadless { get; set; } = false;
    public PedMoveAnimation MoveAnimation { get; set; } = 0;
    public WeaponCollection Weapons { get; set; }
    public bool IsAlive => this.health > 0;
    public Clothing Clothing { get; init; }

    private Player? syncer = null;
    public Player? Syncer
    {
        get => this.syncer;
        set
        {
            if (this.syncer == value)
                return;

            var args = new ElementChangedEventArgs<Ped, Player?>(this, this.Syncer, value, this.IsSync);
            this.syncer = value;
            SyncerChanged?.Invoke(this, args);
        }
    }

    private bool isOnFire = false;
    public bool IsOnFire
    {
        get => this.isOnFire;
        set
        {
            if (this.isOnFire == value)
                return;

            var args = new ElementChangedEventArgs<Ped, bool>(this, this.isOnFire, value, this.IsSync);
            this.isOnFire = value;
            IsOnFireChanged?.Invoke(this, args);
        }
    }
    
    private bool isInWater = false;
    public bool IsInWater
    {
        get => this.isInWater;
        set
        {
            if (this.isInWater == value)
                return;

            var args = new ElementChangedEventArgs<Ped, bool>(this, this.isInWater, value, this.IsSync);
            this.isInWater = value;
            IsInWaterChanged?.Invoke(this, args);
        }
    }

    private Element? target = null;
    public Element? Target
    {
        get => this.target;
        set
        {
            if (this.target == value)
                return;

            var args = new ElementChangedEventArgs<Ped, Element?>(this, this.Target, value, this.IsSync);
            this.target = value;
            TargetChanged?.Invoke(this, args);
        }
    }

    private VehicleAction vehicleAction = VehicleAction.None;
    public VehicleAction VehicleAction
    {
        get => this.vehicleAction;
        set
        {
            if (this.vehicleAction == value)
                return;

            var args = new ElementChangedEventArgs<Ped, VehicleAction>(this, this.vehicleAction, value, this.IsSync);
            this.vehicleAction = value;
            VehicleActionChanged?.Invoke(this, args);
        }
    }

    private Vehicle? jackingVehicle;
    public Vehicle? JackingVehicle
    {
        get => this.jackingVehicle;
        set
        {
            if (this.jackingVehicle == value)
                return;

            var args = new ElementChangedEventArgs<Ped, Vehicle?>(this, this.jackingVehicle, value, this.IsSync);
            this.jackingVehicle = value;
            JackingVehicleChanged?.Invoke(this, args);
        }
    }

    private readonly Dictionary<PedStat, float> stats;


    public Ped(PedModel model, Vector3 position) : base()
    {
        this.Model = (ushort)model;
        this.Position = position;

        this.Clothing = new Clothing(this);
        this.Weapons = new();
        this.stats = new();

        this.Weapons.WeaponAdded += (sender, args) => this.WeaponReceived?.Invoke(this, new WeaponReceivedEventArgs(this, args.Type, args.Ammo, false));
        this.Weapons.WeaponRemoved += (sender, args) => this.WeaponRemoved?.Invoke(this, new WeaponRemovedEventArgs(this, args.Type, args.Ammo));
        this.Weapons.WeaponAmmoUpdated += (sender, args) => this.AmmoUpdated?.Invoke(this, new AmmoUpdateEventArgs(this, args.Type, args.Ammo, args.AmmoInClip, this.IsSync));
        this.Clothing.Changed += (sender, args) => this.ClothingChanged?.Invoke(this, args);
    }

    public new Ped AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
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
        this.HasJetpack = false;
        this.Vehicle = vehicle;
        this.EnteringVehicle = null;
        this.VehicleAction = VehicleAction.None;
    }

    public void AddWeapon(WeaponId weaponId, ushort ammoCount, bool setAsCurrent = false)
    {
        this.Weapons.SilentAdd(new Weapon(weaponId, ammoCount, null));
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
        }
    }

    public void SetAmmoCount(WeaponSlot weaponSlot, ushort count, ushort? inClip)
    {
        var weapon = this.Weapons.FirstOrDefault(weapon => weapon.Slot == weaponSlot);
        if (weapon != null)
            SetAmmoCount(weapon.Type, count, inClip);
    }

    protected void InvokeWasted(PedWastedEventArgs args) => this.Wasted?.Invoke(this, args);

    public virtual void Kill(Element? damager, DamageType damageType, BodyPart bodyPart, ulong animationGroup = 0, ulong animationId = 15)
    {
        this.RunAsSync(() =>
        {
            Reset();
            InvokeWasted(new PedWastedEventArgs(this, damager, damageType, bodyPart, animationGroup, animationId));
        });
    }

    protected void Reset()
    {
        this.health = 0;
        this.JackingVehicle = null;
        this.EnteringVehicle = null;
        this.Vehicle = null;
        this.Seat = null;
        this.VehicleAction = VehicleAction.None;
        this.HasJetpack = false;
    }

    public void Kill(DamageType damageType = DamageType.WEAPONTYPE_UNARMED, BodyPart bodyPart = BodyPart.Torso)
    {
        this.Kill(null, damageType, bodyPart);
    }

    public void SetStat(PedStat stat, float? value)
    {
        var arguments = new PedStatChangedEventArgs(
            stat,
            this.stats.ContainsKey(stat) ? this.stats[stat] : null,
            value);

        if (value == null)
            this.stats.Remove(stat);
        else
            this.stats[stat] = value.Value;

        this.StatChanged?.Invoke(this, arguments);
    }

    public float GetStat(PedStat stat)
    {
        this.stats.TryGetValue(stat, out var value);
        return value;
    }

    public Dictionary<PedStat, float> GetAllStats()
    {
        return this.stats.ToDictionary(x => x.Key, x => x.Value);
    }

    public void SetWeaponStat(WeaponId weapon, float value)
    {
        if (WeaponConstants.WeaponStatsPerWeapon.TryGetValue(weapon, out var stat))
            SetStat(stat, value);
    }

    public float GetWeaponStat(WeaponId weapon)
    {
        if (WeaponConstants.WeaponStatsPerWeapon.TryGetValue(weapon, out var stat))
            return GetStat(stat);
        return 0;
    }

    public void SetAnimation(
        string group,
        string animation,
        TimeSpan? time = null,
        bool loops = true,
        bool updatesPosition = true,
        bool isInteruptable = true,
        bool freezesOnLastFrame = true,
        TimeSpan? blendTime = null,
        bool retainPedState = false)
    {
        if (this.hasJetpack)
            this.HasJetpack = false;

        this.AnimationStarted?.Invoke(this, new PedAnimationStartedEventArgs(
            this,
            group, 
            animation, 
            time ?? TimeSpan.FromMilliseconds(-1), 
            loops,
            updatesPosition,
            isInteruptable,
            freezesOnLastFrame,
            blendTime ?? TimeSpan.FromMilliseconds(250),
            retainPedState));
    }

    public void StopAnimation()
    {
        this.AnimationStopped?.Invoke(this, EventArgs.Empty);
    }

    public void ReloadWeapon()
    {
        this.WeaponReloaded?.Invoke(this, EventArgs.Empty);
    }

    public void SetAnimationProgress(string animation, float progress)
        => this.AnimationProgressChanged?.Invoke(this, new(this, animation, progress));

    public void SetAnimationSpeed(string animation, float speed) 
        => this.AnimationSpeedChanged?.Invoke(this, new(this, animation, speed));

    public void TriggerWeaponAmmoUpdate(WeaponId weapon, ushort ammo, ushort ammoInClip)
        => this.WeaponOrAmmoChanged?.Invoke(this, new(this, weapon, ammo, ammoInClip));

    public event ElementEventHandler<Ped, PedWastedEventArgs>? Wasted;
    public event ElementChangedEventHandler<Ped, ushort>? ModelChanged;
    public event ElementChangedEventHandler<Ped, float>? HealthChanged;
    public event ElementChangedEventHandler<Ped, float>? ArmourChanged;
    public event ElementChangedEventHandler<Ped, WeaponSlot>? WeaponSlotChanged;
    public event ElementChangedEventHandler<Ped, FightingStyle>? FightingStyleChanged;
    public event ElementChangedEventHandler<Ped, bool>? JetpackStateChanged;
    public event ElementChangedEventHandler<Ped, Element?>? TargetChanged;
    public event ElementChangedEventHandler<Ped, Player?>? SyncerChanged;
    public event ElementChangedEventHandler<Ped, float>? GravityChanged;
    public event ElementChangedEventHandler<Ped, bool>? IsOnFireChanged;
    public event ElementChangedEventHandler<Ped, bool>? IsInWaterChanged;
    public event ElementChangedEventHandler<Ped, VehicleAction>? VehicleActionChanged;
    public event ElementChangedEventHandler<Ped, Vehicle?>? JackingVehicleChanged;
    public event ElementChangedEventHandler<Ped, Vehicle?>? VehicleChanged;
    public event ElementChangedEventHandler<Ped, Vehicle?>? EnteringVehicleChanged;
    public event ElementEventHandler<Ped, PedStatChangedEventArgs>? StatChanged;
    public event ElementEventHandler<Ped, WeaponReceivedEventArgs>? WeaponReceived;
    public event ElementEventHandler<Ped, WeaponRemovedEventArgs>? WeaponRemoved;
    public event ElementEventHandler<Ped, AmmoUpdateEventArgs>? AmmoUpdated;
    public event ElementEventHandler<Ped, PedAnimationStartedEventArgs>? AnimationStarted;
    public event ElementEventHandler<Ped, EventArgs>? AnimationStopped;
    public event ElementEventHandler<Ped, PedAnimationProgressChangedEventArgs>? AnimationProgressChanged;
    public event ElementEventHandler<Ped, PedAnimationSpeedChangedEventArgs>? AnimationSpeedChanged;
    public event ElementEventHandler<Ped, EventArgs>? WeaponReloaded;
    public event ElementEventHandler<Ped, PedWeaponOrAmmoChangedEventArgs>? WeaponOrAmmoChanged;
    public event ElementEventHandler<Ped, ClothingChangedEventArgs>? ClothingChanged;
}
