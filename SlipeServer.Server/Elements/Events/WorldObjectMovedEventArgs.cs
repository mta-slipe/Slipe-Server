using SlipeServer.Packets.Definitions.Entities.Structs;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class WorldObjectMovedEventArgs(PositionRotationAnimation movement) : EventArgs
{
    public PositionRotationAnimation Movement { get; } = movement;
}
