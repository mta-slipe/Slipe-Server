using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Server.Elements.Events
{
    public class WeaponReceivedEventArgs : EventArgs
    {
        public Ped Ped { get; set; }
        public WeaponId WeaponId { get; }
        public ushort AmmoCount { get; }
        public bool SetAsCurrent { get; }

        public WeaponReceivedEventArgs(Ped ped, WeaponId weaponId, ushort ammoCount, bool setAsCurrent)
        {
            Ped = ped;
            WeaponId = weaponId;
            AmmoCount = ammoCount;
            SetAsCurrent = setAsCurrent;
        }
    }
}
