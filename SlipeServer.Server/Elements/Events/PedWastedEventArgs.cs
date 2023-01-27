using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PedWastedEventArgs : EventArgs
{
    public Ped Source { get; }
    public Element? Killer { get; }
    public WeaponType WeaponType { get; }
    public BodyPart BodyPart { get; }
    public ulong AnimationGroup { get; }
    public ulong AnimationId { get; }
    public ushort Ammo { get; }

    public PedWastedEventArgs(
        Ped source, Element? killer, WeaponType weaponType, BodyPart bodyPart,
        ulong animationGroup, ulong animationId
    )
    {
        this.Source = source;
        this.Killer = killer;
        this.WeaponType = weaponType;
        this.BodyPart = bodyPart;
        this.AnimationGroup = animationGroup;
        this.AnimationId = animationId;
    }
}
