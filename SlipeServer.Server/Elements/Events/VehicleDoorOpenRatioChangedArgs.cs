using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleDoorOpenRatioChangedArgs : EventArgs
{
    public Vehicle Vehicle { get; }
    public VehicleDoor Door { get; }
    public float Ratio { get; }
    public uint Time { get; }

    public VehicleDoorOpenRatioChangedArgs(Vehicle vehicle, VehicleDoor door, float ratio, uint time)
    {
        this.Vehicle = vehicle;
        this.Door = door;
        this.Ratio = ratio;
        this.Time = time;
    }
}
