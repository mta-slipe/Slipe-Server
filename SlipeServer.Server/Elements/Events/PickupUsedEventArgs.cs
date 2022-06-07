using System;

namespace SlipeServer.Server.Elements.Events;

public class PickupUsedEventArgs : EventArgs
{
    public Player Player { get; }
    public bool IsPickupVisible { get; }

    public PickupUsedEventArgs(Player player, bool isPickupVisible)
    {
        this.Player = player;
        this.IsPickupVisible = isPickupVisible;
    }
}
