using System;

namespace SlipeServer.Server.Elements.Events;

public class VehicleLeftEventArgs : EventArgs
{
    public Ped Ped { get; set; }
    public Vehicle Vehicle { get; set; }
    public byte Seat { get; set; }
    public bool WarpsOut { get; set; }

    public VehicleLeftEventArgs(Ped ped, Vehicle vehicle, byte seat, bool warpsOut)
    {
        this.Ped = ped;
        this.Vehicle = vehicle;
        this.Seat = seat;
        this.WarpsOut = warpsOut;
    }
}
