using System.Numerics;

namespace SlipeServer.Packets.Definitions.Map.Structs;

public struct WorldObjectRemoval
{
    public ushort Model { get; set; }
    public float Radius { get; set; }
    public Vector3 Position { get; set; }
    public byte Interior { get; set; }

    public WorldObjectRemoval(ushort model, float radius, Vector3 position, byte interior)
    {
        this.Model = model;
        this.Radius = radius;
        this.Position = position;
        this.Interior = interior;
    }
}
