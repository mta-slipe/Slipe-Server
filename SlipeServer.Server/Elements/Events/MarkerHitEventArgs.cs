using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class MarkerHitEventArgs(Element element, bool matchingDimension) : EventArgs
{
    public Element HitElement { get; } = element;
    public bool MatchingDimension { get; } = matchingDimension;
}

public sealed class MarkerLeftEventArgs(Element element, bool matchingDimension) : EventArgs
{
    public Element LeftElement { get; } = element;
    public bool MatchingDimension { get; } = matchingDimension;
}
