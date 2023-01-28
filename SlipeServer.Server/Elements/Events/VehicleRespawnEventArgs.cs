using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleRespawnEventArgs : EventArgs
{
    public Vehicle Vehicle { get; }
    public Vector3 RespawnPosition { get; }
    public Vector3 RespawnRotation { get; }

    public VehicleRespawnEventArgs(Vehicle vehicle, Vector3 respawnPosition, Vector3 respawnRotation)
    {
        this.Vehicle = vehicle;
        this.RespawnPosition = respawnPosition;
        this.RespawnRotation = respawnRotation;
    }
}
