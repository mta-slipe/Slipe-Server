using SlipeServer.Server.Elements.Structs;
using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Collections.Events
{
    public class AmmoUpdateEventArgs : EventArgs
    {
        public Weapon Weapon { get; }
        public ushort AmmoCount { get; }
        public ushort? AmmoInClipCount { get; }

        public AmmoUpdateEventArgs(Weapon weapon, ushort ammoCount, ushort? ammoInClipCount)
        {
            this.Weapon = weapon;
            this.AmmoCount = ammoCount;
            this.AmmoInClipCount = ammoInClipCount;
        }
    }
}
