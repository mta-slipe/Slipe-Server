using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Packets.Enums;

public enum PedSyncFlags
{
    Position = 0x01,
    Rotation = 0x02,
    Velocity = 0x04,
    Health = 0x08,
    Armor = 0x10,
    IsOnFire = 0x20,
    IsInWater = 0x40,
}
