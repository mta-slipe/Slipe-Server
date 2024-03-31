namespace SlipeServer.Server.Enums;

public enum WeaponConfigurationFlags
{
    NoAutoAim = 0x01,
    AimArm = 0x02,
    AimFirstPerson = 0x04,
    AimFree = 0x08,
    MoveAndAim = 0x10,
    MoveAndShoot = 0x20,
    Unknown = 0x40,
    Unknown2 = 0x80,
    IsThrowingWeapon = 0x100,
    IsHeavy = 0x200,
    FiresEveryFrame = 0x400,
    CanDualWield = 0x800,
    HasReloadAnimations = 0x1000,
    HasCrouchingAnimations = 0x2000,
    LoopFromEndOfReloadToFireLoopStart = 0x4000,
    ForceLongReload = 0x8000,
    SlowsDown = 0x10000,
    RandomSpeed = 0x20000,
    ForceAnimFinish = 0x40000,
    ShotsExpand = 0x80000
}
