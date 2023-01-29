using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerSubscriptionEventArgs : EventArgs
{
    public Player Player { get; }
    public Element Element { get; }

    public PlayerSubscriptionEventArgs(Player player, Element element)
    {
        this.Player = player;
        this.Element = element;
    }
}
