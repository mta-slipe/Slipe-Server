using SlipeServer.Packets.Enums;
using System.Numerics;


namespace SlipeServer.Packets.Structs;

public struct PedSyncData
{
    public ElementId SourceElementId { get; set; }
    public PedSyncFlags Flags { get; set; }
    public byte TimeSyncContext { get; set; }
    public Vector3? Position { get; set; }
    public float? Rotation { get; set; }
    public Vector3? Velocity { get; set; }
    public float? Health { get; set; }
    public float? Armor { get; set; }
    public bool? IsOnFire { get; set; }
    public bool? IsInWater { get; set; }
}
