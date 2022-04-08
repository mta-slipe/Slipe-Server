using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public class ElementAttachedEventArgs : EventArgs
{
    public Element Source { get; }
    public Element AttachedTo { get; }
    public Vector3 OffsetPosition { get; }
    public Vector3 OffsetRotation { get; }

    public ElementAttachedEventArgs(Element source, Element attachedTo, Vector3 offsetPosition, Vector3 offsetRotation)
    {
        this.Source = source;
        this.AttachedTo = attachedTo;
        this.OffsetPosition = offsetPosition;
        this.OffsetRotation = offsetRotation;
    }
}
