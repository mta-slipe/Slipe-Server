using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Enums;
using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public class CollisionPolygonPointAddedChangedArgs : EventArgs
{
    public CollisionPolygon Polygon { get; set; }
    public int Index { get; set; }
    public Vector2 Position { get; set; }

    public CollisionPolygonPointAddedChangedArgs(CollisionPolygon polygon, int index, Vector2 position)
    {
        this.Polygon = polygon;
        this.Index = index;
        this.Position = position;
    }
}
