using System;
using SlipeServer.Packets;
using System.Net;
using SlipeServer.Packets.Definitions.Entities.Structs;
using System.Numerics;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;

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

        public PlayerWeapon? CurrentWeapon { get; set; }
        public float PedRotation { get; set; } = 0;
        public Vehicle? Vehicle { get; set; }
        public byte? Seat { get; set; }
        public bool HasJetpack { get; set; } = false;
        public bool IsSyncable { get; set; } = true;
        public bool IsHeadless { get; set; } = false;
        public bool IsFrozen { get; set; } = false;
        public PedMoveAnimation MoveAnimation { get; set; } = 0;
        public PedClothing[] Clothes { get; set; }
        public PedWeapon[] Weapons { get; set; }

        public bool IsAlive => health > 0;


        public VehicleAction VehicleAction { get; set; } = VehicleAction.None;
        public Vehicle? JackingVehicle { get; set; }


        public Ped(ushort model, Vector3 position): base()
        {
            this.Model = model;
            this.Position = position;

            this.Clothes = new PedClothing[0];
            this.Weapons = new PedWeapon[0];
        }

        public new Ped AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }

        public void WarpIntoVehicle(Vehicle vehicle)
        {
            if (!this.IsAlive || vehicle.Health <= 0)
                return;

            if (vehicle.Driver != null && vehicle.Driver.VehicleAction != VehicleAction.None)
                return;

            vehicle.Driver?.RemoveFromVehicle();
             
        }

        public void RemoveFromVehicle()
        {

        }

        public event ElementChangedEventHandler<Ped, ushort>? ModelChanged;
        public event ElementChangedEventHandler<Ped, float>? HealthChanged;
        public event ElementChangedEventHandler<Ped, float>? ArmourChanged;
    }
}
