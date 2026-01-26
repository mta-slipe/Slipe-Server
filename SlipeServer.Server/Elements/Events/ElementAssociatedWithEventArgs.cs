using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class ElementAssociatedWithEventArgs : EventArgs
{
    public Element Source { get; }
    public Player? Player { get; }
    public IMtaServer? Server { get; }

    public ElementAssociatedWithEventArgs(Element source, Player player)
    {
        this.Source = source;
        this.Player = player;
    }

    public ElementAssociatedWithEventArgs(Element source, IMtaServer server)
    {
        this.Source = source;
        this.Server = server;
    }
}
