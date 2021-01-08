using SlipeServer.Server.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public class PlayerWastedEventArgs : EventArgs
    {
        public Player Source { get; }
        public Element? Killer { get; }
        public WeaponType WeaponType { get; }
        public BodyPart BodyPart { get; }
        public ulong AnimationGroup { get; }
        public ulong AnimationId { get; }

        public PlayerWastedEventArgs(
            Player source, Element? killer, WeaponType weaponType, BodyPart bodyPart,
            ulong animationGroup, ulong animationId
        )
        {
            Source = source;
            Killer = killer;
            WeaponType = weaponType;
            BodyPart = bodyPart;
            AnimationGroup = animationGroup;
            AnimationId = animationId;
        }
    }
}
