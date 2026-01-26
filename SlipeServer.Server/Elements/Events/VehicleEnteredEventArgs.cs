using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleEnteredEventsArgs(Ped ped, Vehicle vehicle, byte seat, bool warpsIn) : EventArgs
{
    public Ped Ped { get; } = ped;
    public Vehicle Vehicle { get; } = vehicle;
    public byte Seat { get; } = seat;
    public bool WarpsIn { get; } = warpsIn;
}
