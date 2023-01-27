using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleDoorStateChangedArgs : EventArgs
{
    public Vehicle Vehicle { get; }
    public VehicleDoor Door { get; }
    public VehicleDoorState State { get; }
    public bool SpawnFlyingComponent { get; }

    public VehicleDoorStateChangedArgs(Vehicle vehicle, VehicleDoor door, VehicleDoorState state, bool spawnFlyingComponent)
    {
        this.Vehicle = vehicle;
        this.Door = door;
        this.State = state;
        this.SpawnFlyingComponent = spawnFlyingComponent;
    }
}
