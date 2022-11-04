using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public class VehicleWheelStateChangedArgs : EventArgs
{
    public Vehicle Vehicle { get; set; }
    public VehicleWheel Wheel { get; set; }
    public VehicleWheelState State { get; set; }

    public VehicleWheelStateChangedArgs(Vehicle vehicle, VehicleWheel wheel, VehicleWheelState state)
    {
        this.Vehicle = vehicle;
        this.Wheel = wheel;
        this.State = state;
    }
}
