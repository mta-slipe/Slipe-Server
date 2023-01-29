using SlipeServer.Server.Elements.ColShapes;
using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class CollisionPolygonPointPositionChangedArgs : EventArgs
{
    public CollisionPolygon Polygon { get; }
    public uint Index { get; }
    public Vector2 Position { get; }

    public CollisionPolygonPointPositionChangedArgs(CollisionPolygon polygon, uint index, Vector2 position)
    {
        this.Polygon = polygon;
        this.Index = index;
        this.Position = position;
    }
}
