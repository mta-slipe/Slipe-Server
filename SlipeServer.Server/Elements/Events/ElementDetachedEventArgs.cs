using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class ElementDetachedEventArgs : EventArgs
{
    public Element Source { get; }
    public Element AttachedTo { get; }

    public ElementDetachedEventArgs(Element source, Element attachedTo)
    {
        this.Source = source;
        this.AttachedTo = attachedTo;
    }
}
