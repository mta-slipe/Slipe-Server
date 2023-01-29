using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using System.Numerics;

namespace SlipeServer.Server.Extensions.Relaying;

public static class CollisionShapePropertyRelayingExtensions
{
    public static void AddCollisionShapeRelayers(this CollisionShape collisionShape)
    {
        if (collisionShape is CollisionCircle collisionCircle)
        {
            collisionCircle.RadiusChanged += HandleRadiusChange;
        } else if (collisionShape is CollisionSphere collisionSphere)
        {
            collisionSphere.RadiusChanged += HandleRadiusChange;
        } else if (collisionShape is CollisionTube collisionTube)
        {
            collisionTube.RadiusChanged += HandleRadiusChange;
            collisionTube.HeightChanged += HandleHeightChanged;
        } else if (collisionShape is CollisionPolygon collisionPolygon)
        {
            collisionPolygon.HeightChanged += HandlePolygonHeightChanged;
            collisionPolygon.PointPositionChanged += HandlePointPositionChanged;
            collisionPolygon.PointAdded += HandlePointAdded;
            collisionPolygon.PointRemoved += HandlePointRemoved;
        } else if (collisionShape is CollisionRectangle collisionRectangle)
        {
            collisionRectangle.DimensionsChanged += Handle2DDimensionChanged;
        } else if (collisionShape is CollisionCuboid collisionCuboid)
        {
            collisionCuboid.DimensionsChanged += Handle3DDimensionChanged;
        }
    }


    private static void HandlePointAdded(Element sender, CollisionPolygonPointAddedChangedArgs args)
    {
        sender.RelayChange(CollisionShapePacketFactory.CreatePointAdded(args.Polygon, args.Position, args.Index));
    }

    private static void HandlePointRemoved(Element sender, CollisionPolygonPointRemovedChangedArgs args)
    {
        sender.RelayChange(CollisionShapePacketFactory.CreatePointRemoved(args.Polygon, args.Index));
    }

    private static void HandlePointPositionChanged(Element sender, CollisionPolygonPointPositionChangedArgs args)
    {
        sender.RelayChange(CollisionShapePacketFactory.CreatePointPositionChanged(args.Polygon, args.Index, args.Position));
    }

    private static void HandleHeightChanged(Element sender, ElementChangedEventArgs<float> args)
    {
        sender.RelayChange(CollisionShapePacketFactory.CreateSizeChanged(args.Source, new Vector3(args.NewValue, 0, 0)));
    }

    private static void HandlePolygonHeightChanged(Element sender, ElementChangedEventArgs<Vector2> args)
    {
        sender.RelayChange(CollisionShapePacketFactory.CreateSetHeight(args.Source, args.NewValue));
    }

    private static void HandleRadiusChange(Element sender, ElementChangedEventArgs<float> args)
    {
        sender.RelayChange(CollisionShapePacketFactory.CreateSetRadius(args.Source, args.NewValue));
    }

    private static void Handle2DDimensionChanged(Element sender, ElementChangedEventArgs<Vector2> args)
    {
        sender.RelayChange(CollisionShapePacketFactory.CreateSizeChanged(args.Source, new Vector3(args.NewValue, 0)));
    }

    private static void Handle3DDimensionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        sender.RelayChange(CollisionShapePacketFactory.CreateSizeChanged(args.Source, args.NewValue));
    }
}
