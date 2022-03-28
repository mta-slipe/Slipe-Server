using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerSubscriptionEventArgs : EventArgs
{
    public Player Player { get; set; }
    public Element Element { get; set; }

    public PlayerSubscriptionEventArgs(Player player, Element element)
    {
        this.Player = player;
        this.Element = element;
    }
}
