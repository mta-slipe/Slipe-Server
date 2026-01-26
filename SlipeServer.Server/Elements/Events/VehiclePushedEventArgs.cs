using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehiclePushedEventArgs(Player pusher) : EventArgs
{
    public Player Pusher { get; } = pusher;
}
