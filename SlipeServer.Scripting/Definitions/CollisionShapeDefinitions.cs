using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class CollisionShapeDefinitions(IMtaServer server)
{


    [ScriptFunctionDefinition("createColCircle")]
    public CollisionShape CreateColCircle(Vector2 position, float radius)
    {
        return new CollisionCircle(position, radius)
            .AssociateWith(server);
    }

    [ScriptFunctionDefinition("createColCuboid")]
    public CollisionShape CreateColCuboid(Vector3 position, Vector3 dimensions)
    {
        return new CollisionCuboid(position, dimensions)
            .AssociateWith(server);
    }

    [ScriptFunctionDefinition("createColPolygon")]
    public CollisionShape CreateColPolygon(Vector2 position, params Vector2[] vertices)
    {
        return new CollisionPolygon(new Vector3(position.X, position.Y, 0), vertices)
            .AssociateWith(server);
    }

    [ScriptFunctionDefinition("createColRectangle")]
    public CollisionShape CreateCollisionRectangle(Vector2 position, Vector2 dimensions)
    {
        return new CollisionRectangle(position, dimensions)
            .AssociateWith(server);
    }

    [ScriptFunctionDefinition("createColSphere")]
    public CollisionShape CreateCollisionSphere(Vector3 position, float radius)
    {
        return new CollisionSphere(position, radius)
            .AssociateWith(server);
    }

    [ScriptFunctionDefinition("createColTube")]
    public CollisionShape CreateCollisionTube(Vector3 position, float radius, float height)
    {
        return new CollisionTube(position, radius, height)
            .AssociateWith(server);
    }


    [ScriptFunctionDefinition("getColPolygonHeight")]
    public Vector2 GetColPolygonHeight(CollisionPolygon collisionShape)
    {
        return collisionShape.Height;
    }

    [ScriptFunctionDefinition("getColPolygonPoints")]
    public IEnumerable<Vector2> GetColPolygonPoints(CollisionPolygon collisionShape)
    {
        return collisionShape.GetVertices();
    }

    [ScriptFunctionDefinition("getColPolygonPointPosition")]
    public Vector2 GetColPolygonPointPosition(CollisionPolygon collisionShape, int index)
    {
        return collisionShape.GetVertices().ElementAt(index);
    }

    [ScriptFunctionDefinition("getColShapeType")]
    public int GetColShapeType(CollisionShape collisionShape)
    {
        return collisionShape switch
        {
            CollisionCircle => 0,
            CollisionCuboid => 1,
            CollisionSphere => 2,
            CollisionRectangle => 3,
            CollisionPolygon => 4,
            CollisionTube => 5,
            _ => throw new System.Exception($"No coltype defined for {collisionShape.GetType()}")
        };
    }

    [ScriptFunctionDefinition("getColShapeRadius")]
    public float GetColShapeRadius(CollisionShape collisionShape)
    {
        return collisionShape switch
        {
            CollisionCircle circle => circle.Radius,
            CollisionSphere sphere => sphere.Radius,
            CollisionTube tube => tube.Radius,
            _ => throw new System.Exception($"No collision radius defined for {collisionShape.GetType()}")
        };
    }

    [ScriptFunctionDefinition("getColShapeSize")]
    public Vector3 GetColShapeSize(CollisionShape collisionShape)
    {
        return collisionShape switch
        {
            CollisionCuboid cuboid => cuboid.Dimensions,
            CollisionRectangle rectangle => new Vector3(rectangle.Dimensions.X, rectangle.Dimensions.Y, 0),
            CollisionTube tube => new Vector3(tube.Height, 0, 0),
            _ => throw new System.Exception($"No collision radius defined for {collisionShape.GetType()}")
        };
    }

    [ScriptFunctionDefinition("getElementsWithinColShape")]
    public IEnumerable<Element> GetElementsWithinColShape(CollisionShape collisionShape, string? elementType = null)
    {
        return collisionShape.ElementsWithin
            .Where(x => elementType == null || x.ElementType.ToString().Equals(elementType, System.StringComparison.InvariantCultureIgnoreCase));
    }

    [ScriptFunctionDefinition("getElementColShape")]
    public CollisionShape? GetElementColShape(Element element)
    {
        return element switch
        {
            Pickup pickup => pickup.CollisionShape,
            //Marker marker => marker.CollisionShape,
            _ => null
        };
    }

    [ScriptFunctionDefinition("isInsideColShape")]
    public bool IsInsideColShape(CollisionShape collisionShape, Vector3 position)
    {
        return collisionShape.IsWithin(position);
    }

    [ScriptFunctionDefinition("isElementWithinColShape")]
    public bool IsElementWithinColShape(Element element, CollisionShape collisionShape)
    {
        return collisionShape.IsWithin(element);
    }


    [ScriptFunctionDefinition("addColPolygonPoint")]
    public void AddColPolygonPoint(CollisionPolygon collisionShape, Vector2 position, int index = 0)
    {
        collisionShape.AddPoint(position, index - 1);
    }

    [ScriptFunctionDefinition("removeColPolygonPoint")]
    public void RemoveColPolygonPoint(CollisionPolygon collisionShape, int index)
    {
        if (collisionShape.GetPointsCount() <= 3)
            return;

        collisionShape.RemovePoint(index - 1);
    }

    [ScriptFunctionDefinition("setColPolygonHeight")]
    public void SetColPolygonHeight(CollisionPolygon collisionShape, Vector2 height)
    {
        collisionShape.Height = height;
    }

    [ScriptFunctionDefinition("setColPolygonPointPosition")]
    public void SetColPolygonPointPosition(CollisionPolygon collisionShape, int index, Vector2 position)
    {
        collisionShape.SetPointPosition((uint)(index - 1), position);
    }

    [ScriptFunctionDefinition("setColShapeRadius")]
    public void SetColShapeRadius(CollisionShape collisionShape, float radius)
    {
        switch (collisionShape)
        {
            case CollisionSphere sphere:
                sphere.Radius = radius;
                break;

            case CollisionCircle circle:
                circle.Radius = radius;
                break;

            case CollisionTube tube:
                tube.Radius = radius;
                break;

            default:
                throw new System.Exception($"No collision radius defined for {collisionShape.GetType()}");
        };
    }

    [ScriptFunctionDefinition("setColShapeSize")]
    public void SetColShapeSize(CollisionShape collisionShape, float x, float y, float? z = null)
    {
        switch (collisionShape)
        {
            case CollisionCuboid cuboid:
                cuboid.Dimensions = new Vector3(x, y, z ?? 0);
                break;

            case CollisionRectangle rectangle:
                rectangle.Dimensions = new(x, y);
                break;

            case CollisionTube tube:
                tube.Height = x;
                break;

            default:
                throw new System.Exception($"No collision radius defined for {collisionShape.GetType()}");
        };
    }
}
