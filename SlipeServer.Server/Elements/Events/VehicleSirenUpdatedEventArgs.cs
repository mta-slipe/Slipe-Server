using SlipeServer.Packets.Definitions.Entities.Structs;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleSirenUpdatedEventArgs : EventArgs
{
    public Vehicle Vehicle { get; }
    public VehicleSiren Siren { get; }

    public VehicleSirenUpdatedEventArgs(Vehicle vehicle, VehicleSiren siren)
    {
        this.Vehicle = vehicle;
        this.Siren = siren;
    }
}
