using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class ElementAssociatedWithEventArgs : EventArgs
{
    public Element Source { get; }
    public Player? Player { get; }
    public MtaServer? Server { get; }

    public ElementAssociatedWithEventArgs(Element source, Player player)
    {
        this.Source = source;
        this.Player = player;
    }

    public ElementAssociatedWithEventArgs(Element source, MtaServer server)
    {
        this.Source = source;
        this.Server = server;
    }
}
