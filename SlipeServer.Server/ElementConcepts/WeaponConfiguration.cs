using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Numerics;

namespace SlipeServer.Server.ElementConcepts;

/// <summary>
/// Represents configuration for a single weapon, at a specific skill level.
/// </summary>
public struct WeaponConfiguration
{
    public WeaponFireType FireType { get; set; }
    public WeaponId WeaponType { get; set; }
    public float TargetRange { get; set; }
    public float WeaponRange { get; set; }

    public int Flags { get; set; }

    public short Damage { get; set; }

    public float Accuracy { get; set; }
    public float MoveSpeed { get; set; }

    public float AnimationLoopStart { get; set; }
    public float AnimationLoopStop { get; set; }
    public float AnimationLoopBulletFire { get; set; }

    public float Animation2LoopStart { get; set; }
    public float Animation2LoopStop { get; set; }
    public float Animation2LoopBulletFire { get; set; }

    public float AnimationBreakoutTime { get; set; }


    public int Model { get; set; }
    public int Model2 { get; set; }
    public WeaponSlot WeaponSlot { get; set; }

    public ulong AnimationGroup { get; set; }

    public short MaximumClipAmmo { get; set; }
    public Vector3 FireOffset { get; set; }

    public WeaponSkillLevel SkillLevel { get; set; }
    public int RequiredSkillLevelStat { get; set; }

    public float FiringSpeed { get; set; }
    public float Radius { get; set; }
    public float LifeSpan { get; set; }
    public float Spread { get; set; }

    public short AimOffset { get; set; }
    public byte DefaultCombo { get; set; }
    public byte CombosAvailable { get; set; }
}
