using SlipeServer.Server.Elements.ColShapes;
using System;
using System.Numerics;

namespace SlipeServer.Server.Elements.Events;

public sealed class CollisionPolygonPointPositionChangedArgs(CollisionPolygon polygon, uint index, Vector2 position) : EventArgs
{
    public CollisionPolygon Polygon { get; } = polygon;
    public uint Index { get; } = index;
    public Vector2 Position { get; } = position;
}
