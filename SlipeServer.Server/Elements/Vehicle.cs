using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Concepts;
using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementConcepts;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A vehicle element
/// Vehicle elements represent ingame vehicles such as cars, bikes, airplanes, etc.
/// Vechicles are synchronised by clients, usually by the driver, but when unoccupied by a player in close vincinity.
/// </summary>

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Vehicle : Element
{
    public override ElementType ElementType => ElementType.Vehicle;

    protected ushort model;
    public ushort Model
    {
        get => this.model;
        set
        {
            if (this.model == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, ushort>(this, this.Model, value, this.IsSync);
            this.model = value;
            ModelChanged?.Invoke(this, args);
        }
    }

    private float health = 1000;
    public float Health
    {
        get => this.health;
        set
        {
            if (this.health == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, float>(this, this.health, value, this.IsSync);
            this.health = value;
            HealthChanged?.Invoke(this, args);
        }
    }

    public Colors Colors { get; private set; }

    protected byte paintJob = 255;
    public byte PaintJob
    {
        get => this.paintJob;
        set
        {
            if (this.paintJob == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, byte>(this, this.PaintJob, value, this.IsSync);
            this.paintJob = value;
            PaintJobChanged?.Invoke(this, args);
        }
    }

    public VehicleDamage Damage
    {
        get => new()
        {
            Doors = this.doorStates,
            Wheels = this.wheelStates,
            Panels = this.panelStates,
            Lights = this.lightStates
        };
        init
        {
            this.doorStates = value.Doors;
            this.wheelStates = value.Wheels;
            this.panelStates = value.Panels;
            this.lightStates = value.Lights;
        }
    }

    private VehicleVariants variants;
    public VehicleVariants Variants
    {
        get => this.variants;
        set
        {
            var args = new ElementChangedEventArgs<Vehicle, VehicleVariants>(this, this.Variants, value, this.IsSync);
            this.variants = value;
            VariantsChanged?.Invoke(this, args);
        }
    }

    public Vector3 RespawnPosition { get; set; }
    public Vector3 RespawnRotation { get; set; }
    public float RespawnHealth { get; set; } = 1000;

    private Vector2? turretRotation;
    public Vector2? TurretRotation
    {
        get => VehicleConstants.TurretModels.Contains((VehicleModel)this.Model) ? this.turretRotation ?? Vector2.Zero : null;
        set
        {
            if (this.turretRotation == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, Vector2?>(this, this.turretRotation, value, this.IsSync);
            this.turretRotation = value;
            TurretRotationChanged?.Invoke(this, args);
        }
    }

    private ushort? adjustableProperty;
    public ushort? AdjustableProperty
    {
        get => VehicleConstants.AdjustablePropertyModels.Contains((VehicleModel)this.Model) ? this.adjustableProperty ?? 0 : null;
        set => this.adjustableProperty = value;
    }

    private readonly float[] doorRatios;
    public float[] DoorRatios
    {
        get => this.doorRatios;
        set
        {
            if (this.UpdateContext.HasFlag(ElementUpdateContext.Relay))
            {
                for (int i = 0; i < this.doorRatios.Length; i++)
                    if (value.Length > i)
                        this.SetDoorOpenRatio((VehicleDoor)i, value[i]);
            } else
            {
                for (int i = 0; i < this.doorRatios.Length; i++)
                    if (value.Length > i)
                        this.doorRatios[i] = value[i];
            }
        }
    }

    private byte[] doorStates;
    private byte[] wheelStates;
    private byte[] panelStates;
    private byte[] lightStates;

    public VehicleUpgrades Upgrades { get; private set; }

    private string plateText = "";
    public string PlateText
    {
        get => this.plateText;
        set
        {
            if (this.plateText == value)
                return;

            var text = value.Substring(0, Math.Min(value.Length, 8));
            var args = new ElementChangedEventArgs<Vehicle, string>(this, this.PlateText, text, this.IsSync);
            this.plateText = text;
            PlateTextChanged?.Invoke(this, args);
        }
    }

    private VehicleOverrideLights overrideLights = VehicleOverrideLights.None;
    public VehicleOverrideLights OverrideLights
    {
        get => this.overrideLights;
        set
        {
            if (this.overrideLights == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, VehicleOverrideLights>(this, this.overrideLights, value, this.IsSync);
            this.overrideLights = value;
            OverrideLightsChanged?.Invoke(this, args);
        }
    }

    private bool isLandingGearDown = true;
    public bool IsLandingGearDown
    {
        get => this.isLandingGearDown;
        set
        {
            if (this.isLandingGearDown == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.IsLandingGearDown, value, this.IsSync);
            this.isLandingGearDown = value;
            LandingGearChanged?.Invoke(this, args);
        }
    }

    private bool isEngineOn = false;
    public bool IsEngineOn
    {
        get => this.isEngineOn;
        set
        {
            if (this.isEngineOn == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isEngineOn, value, this.IsSync);
            this.isEngineOn = value;
            EngineStateChanged?.Invoke(this, args);
        }
    }

    private bool isLocked = false;
    public bool IsLocked
    {
        get => this.isLocked;
        set
        {
            if (this.isLocked == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isLocked, value, this.IsSync);
            this.isLocked = value;
            LockedStateChanged?.Invoke(this, args);
        }
    }


    private bool areDoorsDamageProof = false;
    public bool AreDoorsDamageProof
    {
        get => this.areDoorsDamageProof;
        set
        {
            if (this.areDoorsDamageProof == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.areDoorsDamageProof, value, this.IsSync);
            this.areDoorsDamageProof = value;
            AreDoorsDamageProofChanged?.Invoke(this, args);
        }
    }

    private bool isDamageProof = false;
    public bool IsDamageProof
    {
        get => this.isDamageProof;
        set
        {
            if (this.isDamageProof == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isDamageProof, value, this.IsSync);
            this.isDamageProof = value;
            IsDamageProofChanged?.Invoke(this, args);
        }
    }

    private bool isDerailed = false;
    public bool IsDerailed
    {
        get => this.isDerailed;
        set
        {
            if (this.isDerailed == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isDerailed, value, this.IsSync);
            this.isDerailed = value;
            IsDerailedChanged?.Invoke(this, args);
        }
    }

    private bool isDerailable = true;
    public bool IsDerailable
    {
        get => this.isDerailable;
        set
        {
            if (this.isDerailable == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isDerailable, value, this.IsSync);
            this.isDerailable = value;
            IsDerailableChanged?.Invoke(this, args);
        }
    }

    private TrainDirection trainDirection = TrainDirection.Clockwise;
    public TrainDirection TrainDirection
    {
        get => this.trainDirection;
        set
        {
            if (this.trainDirection == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, TrainDirection>(this, this.trainDirection, value, this.IsSync);
            this.trainDirection = value;
            TrainDirectionChanged?.Invoke(this, args);
        }
    }


    private bool isTaxiLightOn = false;
    public bool IsTaxiLightOn
    {
        get => this.isTaxiLightOn;
        set
        {
            if (this.isTaxiLightOn == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isTaxiLightOn, value, this.IsSync);
            this.isTaxiLightOn = value;
            TaxiLightStateChanged?.Invoke(this, args);
        }
    }

    private Color headlightColor = Color.White;
    public Color HeadlightColor
    {
        get => this.headlightColor;
        set
        {
            if (this.headlightColor == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, Color>(this, this.headlightColor, value, this.IsSync);
            this.headlightColor = value;
            HeadlightColorChanged?.Invoke(this, args);
        }
    }

    public VehicleHandling DefaultHandling => VehicleHandlingConstants.GetVehicleHandlingFor(this.model);

    private VehicleHandling? handling;
    public VehicleHandling? Handling
    {
        get => this.handling;
        set
        {
            var args = new ElementChangedEventArgs<Vehicle, VehicleHandling?>(this, this.handling, value, this.IsSync);
            this.handling = value;
            HandlingChanged?.Invoke(this, args);
        }
    }

    public VehicleHandling AppliedHandling => this.handling ?? this.DefaultHandling;

    private VehicleSirenSet? sirens;
    public VehicleSirenSet? Sirens
    {
        get => this.sirens;
        set
        {
            if (this.sirens != null)
            {
                this.sirens.Value.SirenAdded -= HandleSirenAdd;
                this.sirens.Value.SirenModified -= HandleSirenModify;
                this.sirens.Value.SirenRemoved -= HandleSirenRemove;
            }

            var args = new ElementChangedEventArgs<Vehicle, VehicleSirenSet?>(this, this.sirens, value, this.IsSync);
            this.sirens = value;
            SirensChanged?.Invoke(this, args);

            if (value != null)
            {
                value.Value.SirenAdded += HandleSirenAdd;
                value.Value.SirenModified += HandleSirenModify;
                value.Value.SirenRemoved += HandleSirenRemove;
            }
        }
    }

    private bool isSirenActive;
    public bool IsSirenActive
    {
        get => this.isSirenActive;
        set
        {
            if (this.isSirenActive == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isSirenActive, value, this.IsSync);
            this.isSirenActive = value;
            AreSirensOnChanged?.Invoke(this, args);
        }
    }

    private void HandleSirenAdd(VehicleSirenSet sirenSet, VehicleSiren sirens)
    {
        this.sirens = sirenSet;
    }
    private void HandleSirenModify(VehicleSirenSet sirenSet, VehicleSiren sirens)
    {
        this.SirenUpdated?.Invoke(this, new VehicleSirenUpdatedEventArgs(this, sirens));
    }

    private void HandleSirenRemove(VehicleSirenSet sirenSet, VehicleSiren sirens)
    {
        this.sirens = sirenSet;
    }

    public bool IsTrailer => VehicleConstants.TrailerModels.Contains((VehicleModel)this.Model);

    public Ped? JackingPed { get; set; }
    public Ped? Driver
    {
        get
        {
            if (this.Occupants.TryGetValue(0, out var value))
                return value;
            return null;
        }
        set
        {
            if (value == null && this.Driver != null)
                this.RemovePassenger(this.Driver, true);
            else if (value != null)
                this.AddPassenger(0, value, true);
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

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isInWater, value, this.IsSync);
            this.isInWater = value;
            IsInWaterChanged?.Invoke(this, args);
        }
    }

    private Player? syncer = null;
    public Player? Syncer
    {
        get => this.syncer;
        set
        {
            if (this.Syncer == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, Player?>(this, this.Syncer, value, this.IsSync);
            this.syncer = value;
            SyncerChanged?.Invoke(this, args);
        }
    }

    public Dictionary<byte, Ped> Occupants { get; set; }

    /// <summary>
    /// Vehicle that is towing this vehicle
    /// </summary>
    public Vehicle? TowingVehicle { get; private set; }

    /// <summary>
    /// Vehicle at the very front of a chain of towing vehicles
    /// </summary>
    public Vehicle? TowingChainHead
    {
        get
        {
            if (this.TowingVehicle != null)
                return this.TowingVehicle.TowingChainHead;
            if (this.TowedVehicle != null)
                return this;
            return null;
        }
    }

    /// <summary>
    /// Vehicle that is being towed by this vehicle
    /// </summary>
    public Vehicle? TowedVehicle { get; private set; }

    public VehicleBlownState BlownState { get; set; }

    private bool isFuelTankExplodable;
    public bool IsFuelTankExplodable
    {
        get => this.isFuelTankExplodable;
        set
        {
            if (this.isFuelTankExplodable == value)
                return;

            var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isFuelTankExplodable, value, this.IsSync);
            this.isFuelTankExplodable = value;
            FuelTankExplodableChanged?.Invoke(this, args);
        }
    }

    public VehicleType VehicleType => VehicleConstants.VehicleTypesPerModel[(VehicleModel)this.model];

    private string DebuggerDisplay => $"{(VehicleModel)this.model} ({this.Id})";

    public Vehicle(ushort model, Vector3 position) : base()
    {
        this.Model = model;
        this.Position = position;
        this.RespawnPosition = position;

        this.Colors = new Colors(this, Color.White, Color.White);
        this.Damage = VehicleDamage.Undamaged;
        this.doorRatios = new float[6];
        this.doorStates = new byte[6];
        this.wheelStates = new byte[4];
        this.panelStates = new byte[7];
        this.lightStates = new byte[4];
        this.Upgrades = new(this);

        this.Name = $"vehicle{this.Id}";
        this.Occupants = new Dictionary<byte, Ped>();

        this.Colors.ColorChanged += (source, args) => this.ColorChanged?.Invoke(this, args);
        this.Upgrades.UpgradeChanged+= (source, args) => this.UpgradeChanged?.Invoke(this, args);
    }

    public Vehicle(VehicleModel model, Vector3 position) : this((ushort)model, position)
    {

    }

    public new Vehicle AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public Ped? GetOccupantInSeat(byte seat)
    {
        if (this.Occupants.TryGetValue(seat, out var value))
            return value;
        return null;
    }

    public void AddPassenger(byte seat, Ped ped, bool warpsIn = true)
    {
        this.Occupants.TryGetValue(seat, out Ped? occupant);
        if (occupant != null && occupant != ped)
        {
            this.RemovePassenger(occupant, true);
        }
        this.Occupants[seat] = ped;
        ped.EnteringVehicle = null;
        ped.Seat = seat;
        ped.Vehicle = this;

        this.PedEntered?.Invoke(this, new VehicleEnteredEventsArgs(ped, this, seat, warpsIn));
    }

    public void RemovePassenger(Ped ped, bool warpsOut = true)
    {
        if (this.Occupants.ContainsValue(ped))
        {
            var item = this.Occupants.First(kvPair => kvPair.Value == ped);

            this.Occupants.Remove(item.Key);
            ped.Vehicle = null;
            ped.EnteringVehicle = null;

            this.PedLeft?.Invoke(this, new VehicleLeftEventArgs(ped, this, item.Key, warpsOut));
        }
    }

    public byte GetMaxPassengers()
    {
        if (VehicleConstants.SeatsPerVehicle.TryGetValue((VehicleModel)this.Model, out byte value))
            return value;
        return 4;
    }

    public byte? GetFreePassengerSeat()
    {
        for (byte seat = 1; seat < this.GetMaxPassengers(); seat++)
        {
            if (!this.Occupants.ContainsKey(seat))
            {
                return seat;
            }
        }
        return null;
    }

    public void BlowUp(bool createExplosion = true)
    {
        this.Health = 0;
        this.IsEngineOn = false;
        this.BlownState = VehicleBlownState.BlownUp;
        this.Blown?.Invoke(this, new VehicleBlownEventArgs(this, createExplosion));
    }

    public void Fix()
    {
        this.BlownState = VehicleBlownState.Intact;
        this.Health = 1000;
        ResetDoorsWheelsPanelsLights();
        this.Fixed?.Invoke(this, new VehicleFixedEventArgs(this));
    }

    public void SetDoorState(VehicleDoor door, VehicleDoorState state, bool spawnFlyingComponent = false)
    {
        this.doorStates[(int)door] = (byte)state;
        this.DoorStateChanged?.Invoke(this, new VehicleDoorStateChangedArgs(this, door, state, spawnFlyingComponent));
    }

    public void SetWheelState(VehicleWheel wheel, VehicleWheelState state)
    {
        this.wheelStates[(int)wheel] = (byte)state;
        this.WheelStateChanged?.Invoke(this, new VehicleWheelStateChangedArgs(this, wheel, state));
    }

    public void SetPanelState(VehiclePanel panel, VehiclePanelState state)
    {
        this.panelStates[(int)panel] = (byte)state;
        this.PanelStateChanged?.Invoke(this, new VehiclePanelStateChangedArgs(this, panel, state));
    }

    public void SetLightState(VehicleLight light, VehicleLightState state)
    {
        this.lightStates[(int)light] = (byte)state;
        this.LightStateChanged?.Invoke(this, new VehicleLightStateChangedArgs(this, light, state));
    }

    public void SetDoorOpenRatio(VehicleDoor door, float ratio, uint time = 0)
    {
        this.doorRatios[(int)door] = ratio;
        this.DoorOpenRatioChanged?.Invoke(this, new VehicleDoorOpenRatioChangedArgs(this, door, ratio, time));
    }

    public void TriggerPushed(Player player)
    {
        this.Pushed?.Invoke(this, new VehiclePushedEventArgs(player));
    }

    public VehicleDoorState GetDoorState(VehicleDoor door) => (VehicleDoorState)this.doorStates[(int)door];
    public VehicleWheelState GetWheelState(VehicleWheel wheel) => (VehicleWheelState)this.wheelStates[(int)wheel];
    public VehiclePanelState GetPanelState(VehiclePanel panel) => (VehiclePanelState)this.panelStates[(int)panel];
    public VehicleLightState GetLightState(VehicleLight light) => (VehicleLightState)this.lightStates[(int)light];
    public float GetDoorOpenRatio(VehicleDoor door) => this.doorRatios[(int)door];

    public void ResetDoorsWheelsPanelsLights()
    {
        foreach (var door in Enum.GetValues(typeof(VehicleDoor)).Cast<VehicleDoor>())
            SetDoorState(door, VehicleDoorState.ShutIntact);
        foreach (var wheel in Enum.GetValues(typeof(VehicleWheel)).Cast<VehicleWheel>())
            SetWheelState(wheel, VehicleWheelState.Inflated);
        foreach (var panel in Enum.GetValues(typeof(VehiclePanel)).Cast<VehiclePanel>())
            SetPanelState(panel, VehiclePanelState.Undamaged);
        foreach (var light in Enum.GetValues(typeof(VehicleLight)).Cast<VehicleLight>())
            SetLightState(light, VehicleLightState.Intact);
        foreach (var door in Enum.GetValues(typeof(VehicleDoor)).Cast<VehicleDoor>())
            SetDoorOpenRatio(door, 0);
    }

    internal void RespawnAt(Vector3 position, Vector3 rotation)
    {
        ResetDoorsWheelsPanelsLights();

        this.IsLandingGearDown = true;
        this.AdjustableProperty = 0;
        this.TurnVelocity = Vector3.Zero;
        this.Velocity = Vector3.Zero;
        this.Position = position;
        this.Rotation = rotation;
        this.Health = this.RespawnHealth;
        this.BlownState = VehicleBlownState.Intact;

        this.Respawned?.Invoke(this, new VehicleRespawnEventArgs(this, position, rotation));
    }

    public void Spawn(Vector3 position, Vector3 rotation)
    {
        RespawnAt(position, rotation);
    }

    public void Respawn()
    {
        RespawnAt(this.RespawnPosition, this.RespawnRotation);
    }

    public void Jack(Ped previousDriver, Ped newDriver)
    {
        this.Jacked?.Invoke(this, new(this, previousDriver, newDriver));
    }

    public void AttachTrailer(Vehicle? trailer, bool updateCounterpart = true)
    {
        if (this.TowedVehicle == trailer)
            return;

        if (updateCounterpart)
        {
            if (trailer != null)
                trailer.AttachToTower(this, false);
            else
                this.TowedVehicle?.AttachToTower(null, false);
        }

        var arguments = new ElementChangedEventArgs<Vehicle, Vehicle?>(this, this.TowedVehicle, trailer, this.IsSync);
        this.TowedVehicle = trailer;
        this.TowedVehicleChanged?.Invoke(this, arguments);
    }

    public void DetachTrailer(bool updateCounterpart = true) => AttachTrailer(null, updateCounterpart);

    public void AttachToTower(Vehicle? tower, bool updateCounterpart = true)
    {
        if (this.TowingVehicle == tower)
            return;

        if (updateCounterpart)
        {
            if (tower != null)
                tower.AttachTrailer(this, false);
            else
                this.TowingVehicle?.AttachTrailer(null, false);
        }
        var arguments = new ElementChangedEventArgs<Vehicle, Vehicle?>(this, this.TowingVehicle, tower, this.IsSync);
        this.TowingVehicle = tower;
        this.TowingVehicleChanged?.Invoke(this, arguments);
    }

    public void DetachFromTower(bool updateCounterpart = true) => AttachToTower(null, updateCounterpart);


    public Func<Ped, Vehicle, byte, bool>? CanEnter;
    public Func<Ped, Vehicle, byte, bool>? CanExit;

    public event ElementEventHandler<VehicleBlownEventArgs>? Blown;
    public event ElementEventHandler<VehicleLeftEventArgs>? PedLeft;
    public event ElementEventHandler<VehicleEnteredEventsArgs>? PedEntered;
    public event ElementChangedEventHandler<Vehicle, ushort>? ModelChanged;
    public event ElementEventHandler<Vehicle, VehicleColorChangedEventsArgs>? ColorChanged;
    public event ElementChangedEventHandler<Vehicle, bool>? LandingGearChanged;
    public event ElementChangedEventHandler<Vehicle, bool>? TaxiLightStateChanged;
    public event ElementChangedEventHandler<Vehicle, Vector2?>? TurretRotationChanged;
    public event ElementChangedEventHandler<Vehicle, string>? PlateTextChanged;
    public event ElementChangedEventHandler<Vehicle, bool>? LockedStateChanged;
    public event ElementChangedEventHandler<Vehicle, bool>? EngineStateChanged;
    public event ElementChangedEventHandler<Vehicle, Player?>? SyncerChanged;
    public event ElementChangedEventHandler<Vehicle, Color>? HeadlightColorChanged;
    public event ElementChangedEventHandler<Vehicle, Vehicle?>? TowedVehicleChanged;
    public event ElementChangedEventHandler<Vehicle, Vehicle?>? TowingVehicleChanged;
    public event ElementChangedEventHandler<Vehicle, bool>? FuelTankExplodableChanged;
    public event ElementChangedEventHandler<Vehicle, byte>? PaintJobChanged;
    public event ElementChangedEventHandler<Vehicle, VehicleVariants>? VariantsChanged;
    public event ElementChangedEventHandler<Vehicle, bool>? IsDamageProofChanged;
    public event ElementChangedEventHandler<Vehicle, bool>? AreDoorsDamageProofChanged;
    public event ElementChangedEventHandler<Vehicle, bool>? IsDerailedChanged;
    public event ElementChangedEventHandler<Vehicle, bool>? IsDerailableChanged;
    public event ElementChangedEventHandler<Vehicle, TrainDirection>? TrainDirectionChanged;
    public event ElementChangedEventHandler<Vehicle, bool>? AreSirensOnChanged;
    public event ElementChangedEventHandler<Vehicle, VehicleSirenSet?>? SirensChanged;
    public event ElementChangedEventHandler<Vehicle, VehicleHandling?>? HandlingChanged;
    public event ElementChangedEventHandler<Vehicle, VehicleOverrideLights>? OverrideLightsChanged;
    public event ElementChangedEventHandler<Vehicle, float>? HealthChanged;
    public event ElementEventHandler<Vehicle, VehicleSirenUpdatedEventArgs>? SirenUpdated;
    public event ElementEventHandler<VehicleRespawnEventArgs>? Respawned;
    public event ElementEventHandler<VehicleDoorStateChangedArgs>? DoorStateChanged;
    public event ElementEventHandler<VehicleWheelStateChangedArgs>? WheelStateChanged;
    public event ElementEventHandler<VehiclePanelStateChangedArgs>? PanelStateChanged;
    public event ElementEventHandler<VehicleLightStateChangedArgs>? LightStateChanged;
    public event ElementEventHandler<VehicleDoorOpenRatioChangedArgs>? DoorOpenRatioChanged;
    public event ElementEventHandler<Vehicle, VehiclePushedEventArgs>? Pushed;
    public event ElementEventHandler<Vehicle, VehicleUpgradeChanged>? UpgradeChanged;
    public event ElementEventHandler<Vehicle, VehicleJackedEventArgs>? Jacked;
    public event ElementChangedEventHandler<Vehicle, bool>? IsInWaterChanged;
    public event ElementEventHandler<VehicleFixedEventArgs>? Fixed;
}
