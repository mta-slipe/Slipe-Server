using SlipeServer.Packets.Enums;
using System.Numerics;


namespace SlipeServer.Packets.Structs;

public struct PedSyncData
{
    public ElementId SourceElementId { get; set; }
    public PedSyncFlags Flags { get; set; }
    public PedSyncFlags2 Flags2 { get; set; }
    public byte TimeSyncContext { get; set; }
    public Vector3? Position { get; set; }
    public float? Rotation { get; set; }
    public Vector3? Velocity { get; set; }
    public float? Health { get; set; }
    public float? Armor { get; set; }
    public bool? IsOnFire { get; set; }
    public bool? IsInWater { get; set; }
    public float? CameraRotation { get; set; }
    public bool IsReloadingWeapon { get; set; }
}

public enum PedSyncFlags2 : byte
{
    HasCameraRotation = 0x01
}
