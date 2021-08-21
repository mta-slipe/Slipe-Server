using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Server.Elements.Events
{
    public class VehicleDamageStateChanged : EventArgs
    {
        public Vehicle Vehicle { get; set; }
        public VehicleDamagePart Part { get; set; }
        public byte Door { get; set; }
        public byte State { get; set; }
        public bool SpawnFlyingComponent { get; set; }

        public VehicleDamageStateChanged(Vehicle vehicle, VehicleDamagePart part, byte door, byte state, bool spawnFlyingComponent)
        {
            this.Vehicle = vehicle;
            this.Part = part;
            this.Door = door;
            this.State = state;
            this.SpawnFlyingComponent = spawnFlyingComponent;
        }
    }
}
