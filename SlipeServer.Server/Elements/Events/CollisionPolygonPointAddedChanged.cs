using SlipeServer.Server.Elements.ColShapes;
using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class CollisionPolygonPointAddedChangedArgs : EventArgs
{
    public CollisionPolygon Polygon { get; }
    public int Index { get; }
    public Vector2 Position { get; }

    public CollisionPolygonPointAddedChangedArgs(CollisionPolygon polygon, int index, Vector2 position)
    {
        this.Polygon = polygon;
        this.Index = index;
        this.Position = position;
    }
}
