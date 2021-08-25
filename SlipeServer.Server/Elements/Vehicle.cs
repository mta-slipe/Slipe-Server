using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Constants;
using SlipeServer.Server.ElementConcepts;
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
        public Colors Colors { get; set; }

        public byte PaintJob { get; set; } = 0;
        public VehicleDamage Damage { get; set; }
        public byte Variant1 { get; set; } = 0;
        public byte Variant2 { get; set; } = 0;

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
        public VehicleUpgrade[] Upgrades { get; set; }
        public string PlateText { get; set; } = "";
        public byte OverrideLights { get; set; } = 0;
        public bool IsLandingGearDown { get; set; } = true;
        public bool IsSirenActive { get; set; } = false;
        public bool IsFuelTankExplodable { get; set; } = false;
        public bool IsEngineOn { get; set; } = false;
        private bool isLocked = false;
        public bool IsLocked
        {
            get => this.isLocked;
            set
            {
                var args = new ElementChangedEventArgs<Vehicle, bool>(this, this.isLocked, value, this.IsSync);
                this.isLocked = value;
                LockedStateChanged?.Invoke(this, args);
            }
        }
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

            this.Colors = new Colors(this, Color.White, Color.White);
            this.Damage = VehicleDamage.Undamaged;
            this.DoorRatios = new float[6];
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

        public virtual bool CanEnter(Ped ped) => true;
        public virtual bool CanExit(Ped ped) => true;

        public event ElementEventHandler? Blown;
        public event ElementEventHandler<VehicleLeftEventArgs>? PedLeft;
        public event ElementEventHandler<VehicleEnteredEventsArgs>? PedEntered;
        public event ElementChangedEventHandler<Vehicle, ushort>? ModelChanged;
        public event ElementChangedEventHandler<Vehicle, bool>? LockedStateChanged;
    }
}
