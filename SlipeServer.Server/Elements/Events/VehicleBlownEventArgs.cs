using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleBlownEventArgs : EventArgs
{
    public Vehicle Vehicle { get; }
    public bool CreateExplosion { get; }

    public VehicleBlownEventArgs(Vehicle vehicle, bool createExplosion)
    {
        this.Vehicle = vehicle;
        this.CreateExplosion = createExplosion;
    }
}
