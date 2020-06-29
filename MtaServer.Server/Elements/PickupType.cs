using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Elements
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
