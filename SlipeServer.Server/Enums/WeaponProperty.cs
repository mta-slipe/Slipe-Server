namespace SlipeServer.Server.Enums;

public enum WeaponProperty
{
    Invalid = 0,
    WeaponRange,
    TargetRange,
    Accuracy,
    Damage,
    LifeSpan,
    FiringSpeed,
    Spread,
    MaxClipAmmo,
    MoveSpeed,
    Flags,
    AnimationGroup,
    TypeSetDisabled,
    FireType,

    Model,
    Model2,

    Slot,

    FireOffset,

    SkillLevel,
    RequiredSkillLevel,

    AnimationLoopStart,
    AnimationLoopStop,
    AnimationLoopReleaseBulletTime,

    Animation2LoopStart,
    Animation2LoopStop,
    Animation2LoopReleaseBulletTime,

    AnimationBreakoutTime,

    Speed,
    Radius,

    AimOffset,

    DefaultCombo,
    CombosAvailable,

    AimNoAutoFlag,            // 0x000001 - cant auto target to aim       (disable automatic up/down adjustment when firing without aiming)
    First = AimNoAutoFlag,
    AimArmFlag,                   // 0x000002 - only needs arm to aim         (ie pistol/shotgun/tec9/uzi)
    AimFirstPersonFlag,            // 0x000004 - uses 1st person aim           (ie sniper/rpg-hs
    AimFreeFlag,                  // 0x000008 - can only use free aiming      (ie country sniper/flame thrower/minigun/fire extinguisher)
    MoveAndAimFlag,              // 0x000010 - can move and aim at same time
    MoveAndFireFlag,            // 0x000020 - can move and fire at same time
    UnknownFlag0x40,
    UnknownFlag0x80,
    IsThrowingWeaponFlag,               // 0x000100 - is a throwing weapon          (ie satchel)
    IsHeavyWeaponFlag,               // 0x000200 - heavy weapon - can't jump     (ie flame thrower/rpgs/minigun)
    FiresEveryFrameFlag,            // 0x000400 - fires every frame within loop (ie paint spray)
    CanDualFlag,                // 0x000800 - can use 2x guns at same time  (ie pistol/shotgun/tec9/uzi)
    HasReloadAnimationsFlag,              // 0x001000 - weapon has reload anims       (ie everything except shotgun/snipers/flame thrower/rpgs/minigun/satchel)
    HasCrouchingANimationsFlag,              // 0x002000 - weapon has crouching anims    (ie everything except flame thrower/rpgs/minigun/satchel)
    LoopReloadAnimationFlag,            // 0x004000 - loop from end of reload to fire loop start
    ForceLongerReloadFlag,            // 0x008000 - force a longer reload time!   (ie rpgs/snipers)
    SlowsDownFlag,                  // 0x010000 - slows down                    (ie flame thrower)
    RandomSpeedFlag,             // 0x020000 - random speed                  (ie flame thrower)
    AbruptAnimationFlag,            // 0x040000 - force the anim to finish player after aim/fire rather than blending out   (ie rpgs)
    ExpandsFlag,                // 0x080000 - expands
    Last = ExpandsFlag,
}
