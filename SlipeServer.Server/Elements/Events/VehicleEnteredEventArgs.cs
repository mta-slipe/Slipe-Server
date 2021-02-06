using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public class VehicleEnteredEventsArgs : EventArgs
    {
        public Ped Ped { get; set; }
        public Vehicle Vehicle { get; set; }
        public byte Seat { get; set; }
        public bool WarpsIn { get; set; }

        public VehicleEnteredEventsArgs(Ped ped, Vehicle vehicle, byte seat, bool warpsIn)
        {
            Ped = ped;
            Vehicle = vehicle;
            Seat = seat;
            WarpsIn = warpsIn;
        }
    }
}
