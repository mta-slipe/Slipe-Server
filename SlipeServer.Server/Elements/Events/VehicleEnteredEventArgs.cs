using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleEnteredEventsArgs : EventArgs
{
    public Ped Ped { get; }
    public Vehicle Vehicle { get; }
    public byte Seat { get; }
    public bool WarpsIn { get; }

    public VehicleEnteredEventsArgs(Ped ped, Vehicle vehicle, byte seat, bool warpsIn)
    {
        this.Ped = ped;
        this.Vehicle = vehicle;
        this.Seat = seat;
        this.WarpsIn = warpsIn;
    }
}
