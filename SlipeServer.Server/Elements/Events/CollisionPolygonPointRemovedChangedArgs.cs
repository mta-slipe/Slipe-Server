using SlipeServer.Server.Elements.ColShapes;
using System;

namespace SlipeServer.Server.Elements.Events;

public class CollisionPolygonPointRemovedChangedArgs : EventArgs
{
    public CollisionPolygon Polygon { get; set; }
    public int Index { get; set; }

    public CollisionPolygonPointRemovedChangedArgs(CollisionPolygon polygon, int index)
    {
        this.Polygon = polygon;
        this.Index = index;
    }
}
