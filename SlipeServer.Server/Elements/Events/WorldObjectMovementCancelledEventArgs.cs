using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class WorldObjectMovementCancelledEventArgs(Vector3 newPosition, Vector3 newRotation) : EventArgs
{
    public Vector3 NewPosition { get; } = newPosition;
    public Vector3 NewRotation { get; } = newRotation;
}
