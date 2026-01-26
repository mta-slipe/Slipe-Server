using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleLightStateChangedArgs(Vehicle vehicle, VehicleLight light, VehicleLightState state) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
    public VehicleLight Light { get; } = light;
    public VehicleLightState State { get; } = state;
}
