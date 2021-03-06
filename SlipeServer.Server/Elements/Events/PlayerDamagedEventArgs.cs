using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public class PlayerDamagedEventArgs : EventArgs
    {
        public Player Source { get; }
        public Element? Damager { get; }
        public WeaponType WeaponType { get; }
        public BodyPart BodyPart { get; }

        public PlayerDamagedEventArgs(Player source, Element? damager, WeaponType weaponType, BodyPart bodyPart)
        {
            Source = source;
            Damager = damager;
            WeaponType = weaponType;
            BodyPart = bodyPart;
        }
    }
}
