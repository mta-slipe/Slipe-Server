using SlipeServer.Server.Elements.ColShapes;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class CollisionPolygonPointRemovedChangedArgs(CollisionPolygon polygon, int index) : EventArgs
{
    public CollisionPolygon Polygon { get; } = polygon;
    public int Index { get; } = index;
}
