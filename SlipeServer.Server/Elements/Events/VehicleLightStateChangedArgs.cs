using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleLightStateChangedArgs : EventArgs
{
    public Vehicle Vehicle { get; }
    public VehicleLight Light { get; }
    public VehicleLightState State { get; }

    public VehicleLightStateChangedArgs(Vehicle vehicle, VehicleLight light, VehicleLightState state)
    {
        this.Vehicle = vehicle;
        this.Light = light;
        this.State = state;
    }
}
