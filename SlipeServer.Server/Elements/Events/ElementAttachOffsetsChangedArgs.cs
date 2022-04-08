using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public class ElementAttachOffsetsChangedArgs : EventArgs
{
    public Element Source { get; set; }
    public Element Target { get; set; }
    public Vector3 OffsetPosition { get; set; }
    public Vector3 OffsetRotation { get; set; }

    public ElementAttachOffsetsChangedArgs(Element source, Element target, Vector3 offsetPosition, Vector3 offsetRotation)
    {
        this.Source = source;
        this.Target = target;
        this.OffsetPosition = offsetPosition;
        this.OffsetRotation = offsetRotation;
    }
}
