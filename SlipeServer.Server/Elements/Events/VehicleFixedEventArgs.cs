using System;

namespace SlipeServer.Server.Elements.Events;

public class VehicleFixedEventArgs(Vehicle vehicle) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
}
