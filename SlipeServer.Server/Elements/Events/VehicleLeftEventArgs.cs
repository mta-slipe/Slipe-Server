using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleLeftEventArgs(Ped ped, Vehicle vehicle, byte seat, bool warpsOut) : EventArgs
{
    public Ped Ped { get; } = ped;
    public Vehicle Vehicle { get; } = vehicle;
    public byte Seat { get; } = seat;
    public bool WarpsOut { get; } = warpsOut;
}
