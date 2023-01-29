using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class VehiclePushedEventArgs : EventArgs
{
    public Player Pusher { get; }

    public VehiclePushedEventArgs(Player pusher)
    {
        this.Pusher = pusher;
    }
}
