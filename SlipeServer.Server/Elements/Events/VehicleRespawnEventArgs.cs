using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehicleRespawnEventArgs(Vehicle vehicle, Vector3 respawnPosition, Vector3 respawnRotation, bool exploded = false) : EventArgs
{
    public Vehicle Vehicle { get; } = vehicle;
    public Vector3 RespawnPosition { get; } = respawnPosition;
    public Vector3 RespawnRotation { get; } = respawnRotation;
    public bool Exploded { get; } = exploded;
}
