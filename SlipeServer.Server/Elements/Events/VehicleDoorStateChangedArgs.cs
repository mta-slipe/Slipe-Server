using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Server.Elements.Events;

public class VehicleDoorStateChangedArgs : EventArgs
{
    public Vehicle Vehicle { get; set; }
    public VehicleDoor Door { get; set; }
    public VehicleDoorState State { get; set; }
    public bool SpawnFlyingComponent { get; set; }

    public VehicleDoorStateChangedArgs(Vehicle vehicle, VehicleDoor door, VehicleDoorState state, bool spawnFlyingComponent)
    {
        this.Vehicle = vehicle;
        this.Door = door;
        this.State = state;
        this.SpawnFlyingComponent = spawnFlyingComponent;
    }
}
