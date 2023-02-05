using SlipeServer.Packets.Structs;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Vehicles;

public struct UnoccupiedVehicleSync
{
    public ElementId Id { get; set; }
    public byte TimeContext { get; set; }
    public UnoccupiedVehicleSyncFlags Flags { get; set; }
    public Vector3? Position { get; set; }
    public Vector3? Rotation { get; set; }
    public Vector3? Velocity { get; set; }
    public Vector3? TurnVelocity { get; set; }
    public float? Health { get; set; }
    public ElementId? Trailer { get; set; }
}
