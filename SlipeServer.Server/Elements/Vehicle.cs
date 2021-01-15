using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Constants;
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
        public Vector2? TurretDirection { get; set; }
        public ushort? AdjustableProperty { get; set; }
        public float[] DoorRatios { get; set; }
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

        public bool IsTrailer => VehicleConstants.TrailerModels.Contains(this.Model);

        public Player? JackingPlayer { get; set; }
        public Ped? Driver
        {
            get
            {
                if (Occupants.ContainsKey(0))
                    return Occupants[0];
                return null;
            }
            set
            {
                if (value == null)
                    Occupants.Remove(0);
                else
                    Occupants[0] = value;
            }
        }
        public Dictionary<byte, Ped> Occupants { get; set; }

        public Vehicle(ushort model, Vector3 position) : base()
        {
            this.Model = model;
            this.Position = position;

            this.Colors = new Color[2] { Color.White, Color.White };
            this.Damage = VehicleDamage.Undamaged;
            this.DoorRatios = new float[6];
            this.Upgrades = new VehicleUpgrade[0];

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

        public void RemovePassenger(Ped ped)
        {
            if (this.Occupants.ContainsValue(ped))
            {
                var item = this.Occupants.First(kvPair => kvPair.Value == ped);

                this.Occupants.Remove(item.Key);
            }
        }

        public byte GetMaxPassengers()
        {
            if (VehicleConstants.SeatsPerVehicle.ContainsKey(this.Model))
                return VehicleConstants.SeatsPerVehicle[this.Model];
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
            this.Blown?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? Blown;
    }
}
