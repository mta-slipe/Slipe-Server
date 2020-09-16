using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements
{
    public class Weapon : WorldObject
    {
        public override ElementType ElementType => ElementType.Weapon;

        public WeaponTargetType TargetType { get; set; }
        public Element? TargetElement { get; set; }
        public byte? BoneTarget { get; set; }
        public byte? WheelTarget { get; set; }
        public Vector3? TargetPosition { get; set; }
        public bool IsChanged { get; set; }
        public ushort? DamagePerHit { get; set; }
        public float? Accuracy { get; set; }
        public float? TargetRange { get; set; }
        public float? WeaponRange { get; set; }
        public bool DisableWeaponModel { get; set; } = false;
        public bool InstantReload { get; set; } = false;
        public bool ShootIfTargetBlocked { get; set; } = false;
        public bool ShootIfTargetOutOfRange { get; set; } = false;
        public bool CheckBuildings { get; set; } = true;
        public bool CheckCarTires { get; set; } = true;
        public bool CheckDummies { get; set; } = true;
        public bool CheckObjects { get; set; } = true;
        public bool CheckPeds { get; set; } = true;
        public bool CheckVehicles { get; set; } = true;
        public bool IgnoreSomeObjectForCamera { get; set; } = true;
        public bool SeeThroughStuff { get; set; } = true;
        public bool ShootThroughStuff { get; set; } = true;
        public WeaponState WeaponState { get; set; }
        public ushort Ammo { get; set; } = 0;
        public ushort ClipAmmo { get; set; } = 0;
        public Element? Owner { get; set; }

        public Weapon(ushort model, Vector3 position) : base(model, position)
        {

        }

        public new Weapon AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
