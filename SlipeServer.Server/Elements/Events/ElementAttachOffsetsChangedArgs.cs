using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class ElementAttachOffsetsChangedArgs(Element source, Element target, Vector3 offsetPosition, Vector3 offsetRotation) : EventArgs
{
    public Element Source { get; } = source;
    public Element Target { get; } = target;
    public Vector3 OffsetPosition { get; } = offsetPosition;
    public Vector3 OffsetRotation { get; } = offsetRotation;
}
