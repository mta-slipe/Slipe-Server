using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Enums;
using System;
using System.Numerics;

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
