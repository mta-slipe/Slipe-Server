using SlipeServer.Server.Elements.ColShapes;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class CollisionShapeHitEventArgs(CollisionShape collisionShape, Element element) : EventArgs
{
    public CollisionShape CollisionShape { get; } = collisionShape;
    public Element Element { get; } = element;
}
