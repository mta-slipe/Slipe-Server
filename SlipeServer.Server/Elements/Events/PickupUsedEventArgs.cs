using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PickupUsedEventArgs(Player player, bool isPickupVisible) : EventArgs
{
    public Player Player { get; } = player;
    public bool IsPickupVisible { get; } = isPickupVisible;
}

public sealed class CancellablePickupUseEventArgs(Player player) : EventArgs
{
    public Player Player { get; } = player;
    public bool Cancel { get; set; }
}
