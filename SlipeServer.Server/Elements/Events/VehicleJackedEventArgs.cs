using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleJackedEventArgs(Vehicle vehicle, Ped previousDriver, Ped newDriver) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
    public Ped PreviousDriver { get; } = previousDriver;
    public Ped NewDriver { get; } = newDriver;
}
