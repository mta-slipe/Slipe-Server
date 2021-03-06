using System;

namespace SlipeServer.Server.Elements.Events
{
    public class VehicleLeftEventArgs : EventArgs
    {
        public Ped Ped { get; set; }
        public Vehicle Vehicle { get; set; }
        public byte Seat { get; set; }
        public bool WarpsOut { get; set; }

        public VehicleLeftEventArgs(Ped ped, Vehicle vehicle, byte seat, bool warpsOut)
        {
            Ped = ped;
            Vehicle = vehicle;
            Seat = seat;
            WarpsOut = warpsOut;
        }
    }
}
