using System;

namespace SlipeServer.Server.Elements.Events
{
    public class WeaponReceivedEventArgs : EventArgs
    {
        public Ped Ped { get; set; }
        public WeaponType WeaponType { get; }
        public ushort AmmoCount { get; }
        public bool SetAsCurrent { get; }

        public WeaponReceivedEventArgs(Ped ped, WeaponType weaponType, ushort ammoCount, bool setAsCurrent)
        {
            Ped = ped;
            WeaponType = weaponType;
            AmmoCount = ammoCount;
            SetAsCurrent = setAsCurrent;
        }
    }
}
