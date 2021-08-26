using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Server.Elements
{
    public class Vehicle : Element
    {
        public override ElementType ElementType => ElementType.Vehicle;

        protected ushort model;
        public ushort Model
        {
            get => this.model;
            set
            {
                var args = new ElementChangedEventArgs<Vehicle, ushort>(this, this.Model, value, this.IsSync);
                this.model = value;
                ModelChanged?.Invoke(this, args);
            }
        }

        public float Health { get; set; } = 1000;
        public Color[] Colors { get; set; }
        public byte PaintJob { get; set; } = 0;
        public VehicleDamage Damage { get; set; }
        public byte Variant1 { get; set; } = 0;
        public byte Variant2 { get; set; } = 0;
        public Vector3 RespawnPosition { get; set; }
        public Vector3 RespawnRotation { get; set; }
        public float RespawnHealth { get; set; }

        private Vector2? turretDirection;
        public Vector2? TurretDirection
        {
            get => VehicleConstants.TurretModels.Contains((VehicleModel)this.Model) ? this.turretDirection ?? Vector2.Zero : null;
            set
            {
                var args = new ElementChangedEventArgs<Vehicle, Vector2?>(this, this.turretDirection, value, this.IsSync);
                this.turretDirection = value;
                TurretDirectionChanged?.Invoke(this, args);
            }
        }

        private ushort? adjustableProperty;
        public ushort? AdjustableProperty
        {
            get => VehicleConstants.AdjustablePropertyModels.Contains((VehicleModel)this.Model) ? this.adjustableProperty ?? 0 : null;
            set => this.adjustableProperty = value;
        }

        public float[] DoorRatios { get; set; }
        private byte[] DoorStates { get; set; }
        private byte[] WheelStates { get; set; }
        private byte[] PanelStates { get; set; }
        private byte[] LightStates { get; set; }
        public VehicleUpgrade[] Upgrades { get; set; }
        public string PlateText { get; set; } = "";
        public byte OverrideLights { get; set; } = 0;

        private bool isLandingGearDown = true;
        public bool IsLandingGearDown
        {
            get => this.isLandingGearDown;
            set
            {
                var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.IsLandingGearDown, value, this.IsSync);
                this.isLandingGearDown = value;
                LandingGearChanged?.Invoke(this, args);
            }
        }

        public bool IsSirenActive { get; set; } = false;
        public bool IsFuelTankExplodable { get; set; } = false;
        public bool IsEngineOn { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public bool AreDoorsUndamageable { get; set; } = false;
        public bool IsDamageProof { get; set; } = false;
        public bool IsFrozen { get; set; } = false;
        public bool IsDerailed { get; set; } = false;
        public bool IsDerailable { get; set; } = true;
        public bool TrainDirection { get; set; } = true;

        private bool isTaxiLightOn = false;
        public bool IsTaxiLightOn
        {
            get => this.isTaxiLightOn;
            set
            {
                var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isTaxiLightOn, value, this.IsSync);
                this.isTaxiLightOn = value;
                TaxiLightStateChanged?.Invoke(this, args);
            }
        }

        public Color HeadlightColor { get; set; } = Color.White;
        public VehicleHandling? Handling { get; set; }
        public VehicleSirenSet? Sirens { get; set; }

        public bool IsTrailer => VehicleConstants.TrailerModels.Contains((VehicleModel)this.Model);

        public Ped? JackingPed { get; set; }
        public Ped? Driver
        {
            get
            {
                if (this.Occupants.ContainsKey(0))
                    return this.Occupants[0];
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
        public Dictionary<byte, Ped> Occupants { get; set; }

        public Vehicle(ushort model, Vector3 position) : base()
        {
            this.Model = model;
            this.Position = position;
            this.RespawnPosition = position;

            this.Colors = new Color[2] { Color.White, Color.White };
            this.Damage = VehicleDamage.Undamaged;
            this.DoorRatios = new float[6];
            this.DoorStates = new byte[6];
            this.WheelStates = new byte[4];
            this.PanelStates = new byte[7];
            this.LightStates = new byte[4];
            this.Upgrades = Array.Empty<VehicleUpgrade>();

            this.Name = $"vehicle{this.Id}";
            this.Occupants = new Dictionary<byte, Ped>();
        }

        public new Vehicle AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public Ped? GetOccupantInSeat(byte seat)
        {
            if (this.Occupants.ContainsKey(seat))
                return this.Occupants[seat];
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

                this.PedLeft?.Invoke(this, new VehicleLeftEventArgs(ped, this, item.Key, warpsOut));
            }
        }

        public byte GetMaxPassengers()
        {
            if (VehicleConstants.SeatsPerVehicle.ContainsKey((VehicleModel)this.Model))
                return VehicleConstants.SeatsPerVehicle[(VehicleModel)this.Model];
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

        public void BlowUp()
        {
            this.Health = 0;
            this.IsEngineOn = false;
            this.Blown?.Invoke(this);
        }

        public void SetDoorState(VehicleDoor door, VehicleDoorState state, bool spawnFlyingComponent = false)
        {
            this.DoorStates[(int)door] = (byte)state;
            this.DoorStateChanged?.Invoke(this, new VehicleDoorStateChangedArgs(this, door, state, spawnFlyingComponent));
        }
        
        public void SetWheelState(VehicleWheel wheel, VehicleWheelState state)
        {
            this.WheelStates[(int)wheel] = (byte)state;
            this.WheelStateChanged?.Invoke(this, new VehicleWheelStateChangedArgs(this, wheel, state));
        }

        public void SetPanelState(VehiclePanel panel, VehiclePanelState state)
        {
            this.PanelStates[(int)panel] = (byte)state;
            this.PanelStateChanged?.Invoke(this, new VehiclePanelStateChangedArgs(this, panel, state));
        }

        public void SetLightState(VehicleLight light, VehicleLightState state)
        {
            this.LightStates[(int)light] = (byte)state;
            this.LightStateChanged?.Invoke(this, new VehicleLightStateChangedArgs(this, light, state));
        }

        public void SetDoorOpenRatio(VehicleDoor door, float ratio, uint time = 0)
        {
            this.DoorRatios[(int)door] = ratio;
            this.DoorOpenRatioChanged?.Invoke(this, new VehicleDoorOpenRatioChangedArgs(this, door, ratio, time));
        }

        public VehicleDoorState GetDoorState(VehicleDoor door) => (VehicleDoorState)this.DoorStates[(int)door];
        public VehicleWheelState GetWheelState(VehicleWheel wheel) => (VehicleWheelState)this.WheelStates[(int)wheel];
        public VehiclePanelState GetPanelState(VehiclePanel panel) => (VehiclePanelState)this.PanelStates[(int)panel];
        public VehicleLightState GetLightState(VehicleLight light) => (VehicleLightState)this.LightStates[(int)light];
        public float GetDoorOpenRatio(VehicleDoor door) => this.DoorRatios[(int)door];

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


        public virtual bool CanEnter(Ped ped) => true;
        public virtual bool CanExit(Ped ped) => true;

        public event ElementEventHandler? Blown;
        public event ElementEventHandler<VehicleLeftEventArgs>? PedLeft;
        public event ElementEventHandler<VehicleEnteredEventsArgs>? PedEntered;
        public event ElementChangedEventHandler<Vehicle, ushort>? ModelChanged;
        public event ElementChangedEventHandler<Vehicle, bool>? LandingGearChanged;
        public event ElementChangedEventHandler<Vehicle, bool>? TaxiLightStateChanged;
        public event ElementChangedEventHandler<Vehicle, Vector2?>? TurretDirectionChanged;
        public event ElementEventHandler<VehicleRespawnEventArgs>? Respawned;
        public event ElementEventHandler<VehicleDoorStateChangedArgs>? DoorStateChanged;
        public event ElementEventHandler<VehicleWheelStateChangedArgs>? WheelStateChanged;
        public event ElementEventHandler<VehiclePanelStateChangedArgs>? PanelStateChanged;
        public event ElementEventHandler<VehicleLightStateChangedArgs>? LightStateChanged;
        public event ElementEventHandler<VehicleDoorOpenRatioChangedArgs>? DoorOpenRatioChanged;
    }
}
