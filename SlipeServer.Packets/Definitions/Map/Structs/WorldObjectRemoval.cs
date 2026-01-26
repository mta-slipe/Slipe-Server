using System.Numerics;

namespace SlipeServer.Packets.Definitions.Map.Structs;

public struct WorldObjectRemoval(ushort model, float radius, Vector3 position, byte interior)
{
    public ushort Model { get; set; } = model;
    public float Radius { get; set; } = radius;
    public Vector3 Position { get; set; } = position;
    public byte Interior { get; set; } = interior;
}
