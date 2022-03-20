namespace SlipeServer.Packets.Definitions.Vehicles
{
    public enum UnoccupiedVehicleSyncFlags
    {
        Position = 0x01,
        Rotation = 0x02,
        Velocity = 0x04,
        TurnVelocity = 0x08,
        Health = 0x10,
        Trailer = 0x20,
        Engine = 0x40,
        Derailed = 0x80,
        IsInWater = 0x100,
    }
}
