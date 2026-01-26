using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleBlownEventArgs(Vehicle vehicle, bool createExplosion) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
    public bool CreateExplosion { get; } = createExplosion;
}
