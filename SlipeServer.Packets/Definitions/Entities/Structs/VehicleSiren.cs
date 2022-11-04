using System.Drawing;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Entities.Structs;

public struct VehicleSiren
{
    public byte Id { get; set; }
    public Vector3 Position { get; set; }
    public Color Color { get; set; }
    public uint SirenMinAlpha { get; set; }
    public bool Is360 { get; set; }
    public bool UsesLineOfSightCheck { get; set; }
    public bool UsesRandomizer { get; set; }
    public bool IsSilent { get; set; }
}
