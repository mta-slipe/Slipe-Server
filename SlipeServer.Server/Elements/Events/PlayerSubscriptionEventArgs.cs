using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerSubscriptionEventArgs(Player player, Element element) : EventArgs
{
    public Player Player { get; } = player;
    public Element Element { get; } = element;
}
