using SlipeServer.Packets.Definitions.Entities.Structs;
using System;

namespace SlipeServer.Server.Elements.Events;

public class VehicleSirenUpdatedEventArgs : EventArgs
{
    public Vehicle Vehicle { get; set; }
    public VehicleSiren Siren { get; set; }

    public VehicleSirenUpdatedEventArgs(Vehicle vehicle, VehicleSiren siren)
    {
        this.Vehicle = vehicle;
        this.Siren = siren;
    }
}
