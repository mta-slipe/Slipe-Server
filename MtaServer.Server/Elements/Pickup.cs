using MtaServer.Server.Elements.Enums;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements
{
    public class Pickup : Element
    {
        public override ElementType ElementType => ElementType.Pickup;

        public ushort Model { get; set; }
        public bool IsVisible { get; set; } = true;
        public PickupType PickupType { get; set; }
        public float? Armor { get; set; }
        public float? Health { get; set; }
        public WeaponType? WeaponType { get; set; }
        public ushort? Ammo{ get; set; }


        public Pickup(Vector3 position, PickupType type, float amount)
        {
            this.Position = position;
            this.PickupType = type;

            if (type == PickupType.Health)
                this.Health = amount;
            else if (type == PickupType.Armor)
                this.Armor = amount;
            else
                throw new Exception($"Can not use health / armor pickup constructor for {type}");
        }

        public Pickup(Vector3 position, WeaponType type, ushort ammo)
        {
            this.Position = position;
            this.Ammo = ammo;
            this.WeaponType = type;

            this.PickupType = PickupType.Weapon;
        }

        public Pickup(Vector3 position, ushort model)
        {
            this.Position = position;
            Model = model;
            this.PickupType = PickupType.Custom;
        }

        public Pickup(Vector3 position, PickupModel model)
        {
            this.Position = position;
            this.Model = (ushort)model;
            this.PickupType = PickupType.Custom;
        }

        public new Pickup AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
