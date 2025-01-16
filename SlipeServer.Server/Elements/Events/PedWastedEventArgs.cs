using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedWastedEventArgs(
    Ped source,
    Element? killer,
    DamageType weaponType,
    BodyPart bodyPart,
    ulong animationGroup,
    ulong animationId) : EventArgs
{
    public Ped Source { get; } = source;
    public Element? Killer { get; } = killer;
    public DamageType WeaponType { get; } = weaponType;
    public BodyPart BodyPart { get; } = bodyPart;
    public ulong AnimationGroup { get; } = animationGroup;
    public ulong AnimationId { get; } = animationId;
    public ushort Ammo { get; }
}
