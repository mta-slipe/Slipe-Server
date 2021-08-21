using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public class VehicleRespawnEventArgs : EventArgs
    {
        public Vehicle Vehicle { get; set; }
        public Vector3 RespawnPosition { get; set; }
        public Vector3 RespawnRotation { get; set; }

        public VehicleRespawnEventArgs(Vehicle vehicle, Vector3 respawnPosition, Vector3 respawnRotation)
        {
            this.Vehicle = vehicle;
            this.RespawnPosition = respawnPosition;
            this.RespawnRotation = respawnRotation;
        }
    }
}
