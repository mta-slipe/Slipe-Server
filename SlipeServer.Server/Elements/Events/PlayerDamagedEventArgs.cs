using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerDamagedEventArgs : EventArgs
{
    public Player Source { get; }
    public Element? Damager { get; }
    public WeaponType WeaponType { get; }
    public BodyPart BodyPart { get; }

    public PlayerDamagedEventArgs(Player source, Element? damager, WeaponType weaponType, BodyPart bodyPart)
    {
        this.Source = source;
        this.Damager = damager;
        this.WeaponType = weaponType;
        this.BodyPart = bodyPart;
    }
}
