using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleWheelStateChangedArgs(Vehicle vehicle, VehicleWheel wheel, VehicleWheelState state) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
    public VehicleWheel Wheel { get; } = wheel;
    public VehicleWheelState State { get; } = state;
}
