using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events
{
    public class AmmoUpdateEventArgs : EventArgs
    {
        public Ped Ped { get; set; }
        public WeaponId WeaponId { get; }
        public ushort AmmoCount { get; }
        public ushort? AmmoInClipCount { get; }

        public AmmoUpdateEventArgs(Ped ped, WeaponId weaponId, ushort ammoCount, ushort? ammoInClipCount)
        {
            Ped = ped;
            WeaponId = weaponId;
            AmmoCount = ammoCount;
            AmmoInClipCount = ammoInClipCount;
        }
    }
}
