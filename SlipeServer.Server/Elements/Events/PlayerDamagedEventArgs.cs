using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerDamagedEventArgs : EventArgs
{
    public Player Source { get; }
    public Element? Damager { get; }
    public DamageType WeaponType { get; }
    public BodyPart BodyPart { get; }

    public PlayerDamagedEventArgs(Player source, Element? damager, DamageType weaponType, BodyPart bodyPart)
    {
        this.Source = source;
        this.Damager = damager;
        this.WeaponType = weaponType;
        this.BodyPart = bodyPart;
    }
}
