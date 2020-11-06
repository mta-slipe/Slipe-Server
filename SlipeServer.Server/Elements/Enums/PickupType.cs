using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements
{
    public enum PickupType
    {
        Health,
        Armor,
        Weapon,
        Custom,
        Invalid = 0xFF
    }
}
