using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for triggering collision shape enter and exit events when an element's position changes
/// </summary>
public class CollisionShapeBehaviour
{
    private readonly HashSet<CollisionShape> collisionShapes;
    private readonly MtaServer server;

    public CollisionShapeBehaviour(MtaServer server, IElementCollection elementCollection)
    {
        this.collisionShapes = new HashSet<CollisionShape>();
        foreach (var collisionShape in elementCollection.GetByType<CollisionShape>(ElementType.Colshape))
        {
            this.AddCollisionShape(collisionShape);
        }

        server.ElementCreated += OnElementCreate;
        this.server = server;
    }

    private void OnElementCreate(Element element)
    {
        if (element is CollisionShape collisionShape)
        {
            AddCollisionShape(collisionShape);
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
        } else
        {
            element.PositionChanged += OnElementPositionChange;
        }
    }

    private void HandlePointAdded(Element sender, CollisionPolygonPointAddedChangedArgs args)
    {
        this.server.BroadcastPacket(CollisionShapePacketFactory.CreatePointAdded(args.Polygon, args.Position, args.Index));
    }

    private void HandlePointRemoved(Element sender, CollisionPolygonPointRemovedChangedArgs args)
    {
        this.server.BroadcastPacket(CollisionShapePacketFactory.CreatePointRemoved(args.Polygon, args.Index));
    }

    private void HandlePointPositionChanged(Element sender, CollisionPolygonPointPositionChangedArgs args)
    {
        this.server.BroadcastPacket(CollisionShapePacketFactory.CreatePointPositionChanged(args.Polygon, args.Index, args.Position));
    }

    private void HandleHeightChanged(Element sender, ElementChangedEventArgs<float> args)
    {
        this.server.BroadcastPacket(CollisionShapePacketFactory.CreateSizeChanged(args.Source, new Vector3(args.NewValue, 0, 0)));
    }

    private void HandlePolygonHeightChanged(Element sender, ElementChangedEventArgs<Vector2> args)
    {
        this.server.BroadcastPacket(CollisionShapePacketFactory.CreateSetHeight(args.Source, args.NewValue));
    }

    private void HandleRadiusChange(Element sender, ElementChangedEventArgs<float> args)
    {
        this.server.BroadcastPacket(CollisionShapePacketFactory.CreateSetRadius(args.Source, args.NewValue));
    }

    private void Handle2DDimensionChanged(Element sender, ElementChangedEventArgs<Vector2> args)
    {
        this.server.BroadcastPacket(CollisionShapePacketFactory.CreateSizeChanged(args.Source, new Vector3(args.NewValue, 0)));
    }

    private void Handle3DDimensionChanged(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        this.server.BroadcastPacket(CollisionShapePacketFactory.CreateSizeChanged(args.Source, args.NewValue));
    }

    private void AddCollisionShape(CollisionShape collisionShape)
    {
        this.collisionShapes.Add(collisionShape);
        collisionShape.Destroyed += (source) => this.collisionShapes.Remove(collisionShape);
    }

    private void RefreshColliders(Element element)
    {
        foreach (var shape in this.collisionShapes)
        {
            shape.CheckElementWithin(element);
        }
    }

    private void OnElementPositionChange(object sender, ElementChangedEventArgs<Vector3> eventArgs)
    {
        eventArgs.Source.RunWithContext(
            () => RefreshColliders(eventArgs.Source), 
            Elements.Enums.ElementUpdateContext.PostEvent
        );
    }
}
