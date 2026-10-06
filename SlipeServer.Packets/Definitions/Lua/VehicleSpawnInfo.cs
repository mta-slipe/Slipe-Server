using SlipeServer.Packets.Structs;
using System;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua;

public class VehicleSpawnInfo
{
    public ElementId ElementId { get; set; }
    public byte TimeContext { get; set; }
    public ushort VehicleId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Color?[] Colors { get; set; } = Array.Empty<Color?>();
}
