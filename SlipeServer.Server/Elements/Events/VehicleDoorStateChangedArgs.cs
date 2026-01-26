using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleDoorStateChangedArgs(Vehicle vehicle, VehicleDoor door, VehicleDoorState state, bool spawnFlyingComponent) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
    public VehicleDoor Door { get; } = door;
    public VehicleDoorState State { get; } = state;
    public bool SpawnFlyingComponent { get; } = spawnFlyingComponent;
}
