using MtaServer.Packets.Definitions.Entities.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements
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
        public byte Alpha { get; set; } = 255;
        public Color HeadlightColor { get; set; } = Color.White;
        public VehicleHandling? Handling { get; set; }
        public VehicleSirenSet? Sirens { get; set; }



        public Vehicle(ushort model, Vector3 position): base()
        {
            this.Model = model;
            this.Position = position;

            this.Colors = new Color[2] { Color.White, Color.White };
            this.Damage = VehicleDamage.Undamaged;
            this.DoorRatios = new float[6];
            this.Upgrades = new VehicleUpgrade[0];

            this.Name = $"vehicle{this.Id}";
        }

        public new Vehicle AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
