using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleDoorOpenRatioChangedArgs(Vehicle vehicle, VehicleDoor door, float ratio, uint time) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
    public VehicleDoor Door { get; } = door;
    public float Ratio { get; } = ratio;
    public uint Time { get; } = time;
}
