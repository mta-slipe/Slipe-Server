using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class ElementAttachedEventArgs(Element source, Element attachedTo, Vector3 offsetPosition, Vector3 offsetRotation) : EventArgs
{
    public Element Source { get; } = source;
    public Element AttachedTo { get; } = attachedTo;
    public Vector3 OffsetPosition { get; } = offsetPosition;
    public Vector3 OffsetRotation { get; } = offsetRotation;
}
