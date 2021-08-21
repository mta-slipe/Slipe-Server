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

        public ushort Model { get; set; }
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
        public Vector2? TurretRotation
        {
            get => VehicleConstants.TurretModels.Contains((VehicleModel)this.Model) ? this.turretDirection ?? Vector2.Zero : null;
            set => this.turretDirection = value;
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
        public bool IsLandingGearDown { get; set; } = true;
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
        public bool IsTaxiLightOn { get; set; } = false;
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

        public void SetTowedByVehicle(Vehicle? vehicle)
        {
            // not implemented yet
        }

        public void AttachTo(Element? vehicle)
        {
            // not implemented yet
        }

        public void SetDoorState(VehicleDoor door, VehicleDoorState state, bool spawnFlyingComponent = false)
        {
            this.DoorStates[(byte)door] = (byte)state;
            DamageStateChanged?.Invoke(this, new VehicleDamageStateChanged(this, VehicleDamagePart.Door, (byte)door, (byte)state, spawnFlyingComponent));
        }


        internal void ResetWheelsPanelsLights()
        {
            Array.Clear(this.WheelStates, 0, this.WheelStates.Length);
            Array.Clear(this.PanelStates, 0, this.PanelStates.Length);
            Array.Clear(this.LightStates, 0, this.LightStates.Length);
        }

        internal void ResetDoors()
        {
            Array.Clear(this.DoorRatios, 0, this.DoorRatios.Length);
        }

        internal void RespawnAt(Vector3 position, Vector3 rotation)
        {
            this.Respawned?.Invoke(this, new VehicleRespawnEventArgs(this, position, rotation));

            ResetDoors();
            ResetWheelsPanelsLights();
            SetTowedByVehicle(null);
            AttachTo(null);

            this.IsLandingGearDown = true;
            this.AdjustableProperty = 0;
            this.TurnVelocity = Vector3.Zero;
            this.Velocity = Vector3.Zero;
            this.Position = position;
            this.Rotation = rotation;
            this.Health = this.RespawnHealth;
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
        public event ElementEventHandler<VehicleRespawnEventArgs>? Respawned;
        public event ElementEventHandler<VehicleDamageStateChanged>? DamageStateChanged;
    }
}
