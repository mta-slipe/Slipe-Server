using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public class VehicleDoorOpenRatioChangedArgs : EventArgs
    {
        public Vehicle Vehicle { get; set; }
        public VehicleDoor Door { get; set; }
        public float Ratio { get; set; }
        public uint Time { get; }

        public VehicleDoorOpenRatioChangedArgs(Vehicle vehicle, VehicleDoor door, float ratio, uint time)
        {
            this.Vehicle = vehicle;
            this.Door = door;
            this.Ratio = ratio;
            this.Time = time;
        }
    }
}
