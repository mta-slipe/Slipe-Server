using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleLeftEventArgs : EventArgs
{
    public Ped Ped { get; }
    public Vehicle Vehicle { get; }
    public byte Seat { get; }
    public bool WarpsOut { get; }

    public VehicleLeftEventArgs(Ped ped, Vehicle vehicle, byte seat, bool warpsOut)
    {
        this.Ped = ped;
        this.Vehicle = vehicle;
        this.Seat = seat;
        this.WarpsOut = warpsOut;
    }
}
