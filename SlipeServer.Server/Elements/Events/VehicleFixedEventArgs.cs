using System;

namespace SlipeServer.Server.Elements.Events;

public class VehicleFixedEventArgs : EventArgs
{
    public Vehicle Vehicle { get; }

    public VehicleFixedEventArgs(Vehicle vehicle)
    {
        this.Vehicle = vehicle;
    }
}
