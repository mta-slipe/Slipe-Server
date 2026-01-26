using System.Numerics;

namespace SlipeServer.Server.Elements;

/// <summary>
/// Weapon object element
/// A weapon object is a physical object that represents a weapon (usually a firearm), and is able to fire and reload. Much like a player's weapon.
/// </summary>
public class WeaponObject(ushort model, Vector3 position) : WorldObject(model, position)
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

    public new WeaponObject AssociateWith(IMtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }
}
