using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PickupHitEventArgs(Player player) : EventArgs
{
    public Player Player { get; } = player;
}

public sealed class PickupLeftEventArgs(Player player) : EventArgs
{
    public Player Player { get; } = player;
}
