using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class ElementAttachOffsetsChangedArgs : EventArgs
{
    public Element Source { get; }
    public Element Target { get; }
    public Vector3 OffsetPosition { get; }
    public Vector3 OffsetRotation { get; }

    public ElementAttachOffsetsChangedArgs(Element source, Element target, Vector3 offsetPosition, Vector3 offsetRotation)
    {
        this.Source = source;
        this.Target = target;
        this.OffsetPosition = offsetPosition;
        this.OffsetRotation = offsetRotation;
    }
}
