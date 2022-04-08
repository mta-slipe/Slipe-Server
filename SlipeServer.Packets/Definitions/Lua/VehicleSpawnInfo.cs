using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Packets.Definitions.Lua;

public class VehicleSpawnInfo
{
    public uint ElementId { get; set; }
    public byte TimeContext { get; set; }
    public ushort VehicleId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Color[] Colors { get; set; } = Array.Empty<Color>();
}
