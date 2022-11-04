using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public class VehicleLightStateChangedArgs : EventArgs
{
    public Vehicle Vehicle { get; set; }
    public VehicleLight Light { get; set; }
    public VehicleLightState State { get; set; }

    public VehicleLightStateChangedArgs(Vehicle vehicle, VehicleLight light, VehicleLightState state)
    {
        this.Vehicle = vehicle;
        this.Light = light;
        this.State = state;
    }
}
