using SlipeServer.Packets.Definitions.Entities.Structs;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleSirenUpdatedEventArgs(Vehicle vehicle, VehicleSiren siren) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
    public VehicleSiren Siren { get; } = siren;
}
