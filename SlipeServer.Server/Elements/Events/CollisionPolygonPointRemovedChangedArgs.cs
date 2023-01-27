using SlipeServer.Server.Elements.ColShapes;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class CollisionPolygonPointRemovedChangedArgs : EventArgs
{
    public CollisionPolygon Polygon { get; }
    public int Index { get; }

    public CollisionPolygonPointRemovedChangedArgs(CollisionPolygon polygon, int index)
    {
        this.Polygon = polygon;
        this.Index = index;
    }
}
