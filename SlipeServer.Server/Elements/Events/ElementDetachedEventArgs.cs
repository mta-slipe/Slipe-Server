using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class ElementDetachedEventArgs(Element source, Element attachedTo) : EventArgs
{
    public Element Source { get; } = source;
    public Element AttachedTo { get; } = attachedTo;
}
