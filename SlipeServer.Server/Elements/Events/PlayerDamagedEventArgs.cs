using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerDamagedEventArgs(
    Player source, 
    Element? damager, 
    DamageType weaponType, 
    BodyPart bodyPart, 
    float loss) : EventArgs
{
    public Player Source { get; } = source;
    public Element? Damager { get; } = damager;
    public DamageType WeaponType { get; } = weaponType;
    public BodyPart BodyPart { get; } = bodyPart;
    public float Loss { get; } = loss;
}
