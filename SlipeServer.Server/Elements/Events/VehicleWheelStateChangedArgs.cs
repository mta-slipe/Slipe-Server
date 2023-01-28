using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleWheelStateChangedArgs : EventArgs
{
    public Vehicle Vehicle { get; }
    public VehicleWheel Wheel { get; }
    public VehicleWheelState State { get; }

    public VehicleWheelStateChangedArgs(Vehicle vehicle, VehicleWheel wheel, VehicleWheelState state)
    {
        this.Vehicle = vehicle;
        this.Wheel = wheel;
        this.State = state;
    }
}
