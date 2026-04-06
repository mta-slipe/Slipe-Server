using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class ElementScriptDefinitions(IElementCollection elementCollection, IMtaServer server)
{
    [ScriptFunctionDefinition("destroyElement")]
    public void DestroyElement(Element element) => element.Destroy();

    [ScriptFunctionDefinition("createElement")]
    public Element CreateElement(string elementType, string? elementId = null)
    {
        var element = new DummyElement
        {
            ElementTypeName = elementType,
            Name = elementId ?? string.Empty
        };
        element.AssociateWith(server);
        return element;
    }

    [ScriptFunctionDefinition("cloneElement")]
    public Element? CloneElement(Element theElement, float xPos = 0, float yPos = 0, float zPos = 0, bool cloneChildren = false)
    {
        Element? clone = theElement switch
        {
            WorldObject obj => new WorldObject(obj.Model, obj.Position + new Vector3(xPos, yPos, zPos))
            {
                Rotation = obj.Rotation,
                Scale = obj.Scale,
                IsLowLod = obj.IsLowLod,
                DoubleSided = obj.DoubleSided,
            },
            Ped ped => new Ped((PedModel)ped.Model, ped.Position + new Vector3(xPos, yPos, zPos))
            {
                Rotation = ped.Rotation,
            },
            _ => null
        };

        if (clone == null)
            return null;

        clone.Interior = theElement.Interior;
        clone.Dimension = theElement.Dimension;
        clone.Alpha = theElement.Alpha;
        clone.IsFrozen = theElement.IsFrozen;
        clone.AssociateWith(server);
        return clone;
    }

    [ScriptFunctionDefinition("isElement")]
    public bool IsElement(Element? element = null) => element != null && !element.IsDestroyed;

    [ScriptFunctionDefinition("getRootElement")]
    public Element GetRootElement() => server.RootElement;

    [ScriptFunctionDefinition("getElementByID")]
    public Element? GetElementById(string id, int index = 0)
        => elementCollection.GetAll().Where(e => e.Name == id).ElementAtOrDefault(index);

    [ScriptFunctionDefinition("getElementByIndex")]
    public Element? GetElementByIndex(string theType, int index)
        => GetElementsByTypeName(theType).ElementAtOrDefault(index);

    [ScriptFunctionDefinition("getElementsByType")]
    public IEnumerable<Element> GetElementsByType(string theType, Element? startAt = null)
    {
        var elements = GetElementsByTypeName(theType);
        if (startAt != null)
            elements = elements.Where(e => e == startAt || e.IsChildOf(startAt));
        return elements;
    }

    [ScriptFunctionDefinition("getElementsWithinRange")]
    public IEnumerable<Element> GetElementsWithinRange(float x, float y, float z, float range, string? elemType = null, int? interior = null, int? dimension = null)
    {
        var position = new Vector3(x, y, z);
        IEnumerable<Element> elements = string.IsNullOrEmpty(elemType)
            ? elementCollection.GetWithinRange(position, range)
            : elemType switch
            {
                "player" => elementCollection.GetWithinRange<Player>(position, range),
                "vehicle" => elementCollection.GetWithinRange<Vehicle>(position, range),
                "object" => elementCollection.GetWithinRange<WorldObject>(position, range),
                "ped" => elementCollection.GetWithinRange<Ped>(position, range).Where(p => p is not Player),
                "marker" => elementCollection.GetWithinRange<Marker>(position, range),
                "blip" => elementCollection.GetWithinRange<Blip>(position, range),
                "colshape" => elementCollection.GetWithinRange<CollisionShape>(position, range),
                "radararea" => elementCollection.GetWithinRange<RadarArea>(position, range),
                "pickup" => elementCollection.GetWithinRange<Pickup>(position, range),
                "team" => elementCollection.GetWithinRange<Team>(position, range),
                _ => elementCollection.GetWithinRange(position, range).Where(e => GetElementTypeName(e) == elemType)
            };

        if (interior.HasValue)
            elements = elements.Where(e => e.Interior == interior.Value);
        if (dimension.HasValue)
            elements = elements.Where(e => e.Dimension == dimension.Value);

        return elements;
    }


    [ScriptFunctionDefinition("getElementPosition")]
    public Vector3 GetElementPosition(Element element) => element.Position;

    [ScriptFunctionDefinition("setElementPosition")]
    public bool SetElementPosition(Element element, Vector3 position)
    {
        element.Position = position;
        return true;
    }

    [ScriptFunctionDefinition("getElementRotation")]
    public Vector3 GetElementRotation(Element element) => element.Rotation;

    [ScriptFunctionDefinition("setElementRotation")]
    public bool SetElementRotation(Element element, Vector3 rotation)
    {
        element.Rotation = rotation;
        return true;
    }

    [ScriptFunctionDefinition("getElementVelocity")]
    public Vector3 GetElementVelocity(Element element) => element.Velocity;

    [ScriptFunctionDefinition("setElementVelocity")]
    public bool SetElementVelocity(Element element, float vx, float vy, float vz)
    {
        element.Velocity = new Vector3(vx, vy, vz);
        return true;
    }

    [ScriptFunctionDefinition("getElementAngularVelocity")]
    public Vector3 GetElementAngularVelocity(Element element) => element.TurnVelocity;

    [ScriptFunctionDefinition("setElementAngularVelocity")]
    public bool SetElementAngularVelocity(Element element, float rx, float ry, float rz)
    {
        element.TurnVelocity = new Vector3(rx, ry, rz);
        return true;
    }

    [ScriptFunctionDefinition("getElementMatrix")]
    public LuaValue GetElementMatrix(Element element, bool legacy = true)
    {
        var pos = element.Position;
        var rot = element.Rotation;

        float rx = MathF.PI * rot.X / 180f;
        float ry = MathF.PI * rot.Y / 180f;
        float rz = MathF.PI * rot.Z / 180f;

        var m = new float[4, 4];

        m[0, 0] = MathF.Cos(rz) * MathF.Cos(ry) - MathF.Sin(rz) * MathF.Sin(rx) * MathF.Sin(ry);
        m[0, 1] = MathF.Cos(ry) * MathF.Sin(rz) + MathF.Cos(rz) * MathF.Sin(rx) * MathF.Sin(ry);
        m[0, 2] = -MathF.Cos(rx) * MathF.Sin(ry);
        m[0, 3] = 0;

        m[1, 0] = -MathF.Cos(rx) * MathF.Sin(rz);
        m[1, 1] = MathF.Cos(rz) * MathF.Cos(rx);
        m[1, 2] = MathF.Sin(rx);
        m[1, 3] = 0;

        m[2, 0] = MathF.Cos(rz) * MathF.Sin(ry) + MathF.Cos(ry) * MathF.Sin(rz) * MathF.Sin(rx);
        m[2, 1] = MathF.Sin(rz) * MathF.Sin(ry) - MathF.Cos(rz) * MathF.Cos(ry) * MathF.Sin(rx);
        m[2, 2] = MathF.Cos(rx) * MathF.Cos(ry);
        m[2, 3] = 0;

        m[3, 0] = pos.X;
        m[3, 1] = pos.Y;
        m[3, 2] = pos.Z;
        m[3, 3] = 1;

        var rows = new Dictionary<LuaValue, LuaValue>();
        for (int i = 0; i < 4; i++)
        {
            var row = new Dictionary<LuaValue, LuaValue>();
            for (int j = 0; j < 4; j++)
                row[new LuaValue(j + 1)] = new LuaValue(m[i, j]);
            rows[new LuaValue(i + 1)] = new LuaValue(row);
        }
        return new LuaValue(rows);
    }


    [ScriptFunctionDefinition("getElementType")]
    public string GetElementType(Element element) => GetElementTypeName(element);

    [ScriptFunctionDefinition("getElementID")]
    public string GetElementId(Element element) => element.Name;

    [ScriptFunctionDefinition("setElementID")]
    public bool SetElementId(Element element, string id)
    {
        element.Name = id;
        return true;
    }

    [ScriptFunctionDefinition("getElementModel")]
    public ushort GetElementModel(Element element) => element switch
    {
        WorldObject worldObject => worldObject.Model,
        Vehicle vehicle => vehicle.Model,
        Ped ped => ped.Model,
        _ => throw new Exception("Unsupported element")
    };

    [ScriptFunctionDefinition("setElementModel")]
    public bool SetElementModel(Element element, ushort model)
    {
        switch (element)
        {
            case WorldObject worldObject:
                worldObject.Model = model;
                return true;
            case Vehicle vehicle:
                vehicle.Model = model;
                return true;
            case Ped ped:
                ped.Model = model;
                return true;
            default:
                return false;
        }
    }

    [ScriptFunctionDefinition("getElementAlpha")]
    public byte GetElementAlpha(Element element) => element.Alpha;

    [ScriptFunctionDefinition("setElementAlpha")]
    public bool SetElementAlpha(Element element, byte alpha)
    {
        element.Alpha = alpha;
        return true;
    }

    [ScriptFunctionDefinition("getElementDimension")]
    public ushort GetElementDimension(Element element) => element.Dimension;

    [ScriptFunctionDefinition("setElementDimension")]
    public bool SetElementDimension(Element element, int dimension)
    {
        element.Dimension = (ushort)dimension;
        return true;
    }

    [ScriptFunctionDefinition("getElementInterior")]
    public byte GetElementInterior(Element element) => element.Interior;

    [ScriptFunctionDefinition("setElementInterior")]
    public bool SetElementInterior(Element element, int interior)
    {
        element.Interior = (byte)interior;
        return true;
    }

    [ScriptFunctionDefinition("getElementHealth")]
    public float GetElementHealth(Element element) => element switch
    {
        Ped ped => ped.Health,
        WorldObject obj => obj.Health,
        Vehicle vehicle => vehicle.Health,
        _ => throw new Exception("Unsupported element type for getElementHealth")
    };

    [ScriptFunctionDefinition("setElementHealth")]
    public bool SetElementHealth(Element element, float health)
    {
        switch (element)
        {
            case Ped ped:
                ped.Health = health;
                return true;
            case WorldObject obj:
                obj.Health = health;
                return true;
            case Vehicle vehicle:
                vehicle.Health = health;
                return true;
            default:
                return false;
        }
    }

    [ScriptFunctionDefinition("getElementParent")]
    public Element? GetElementParent(Element element) => element.Parent as Element;

    [ScriptFunctionDefinition("setElementParent")]
    public bool SetElementParent(Element element, Element parent)
    {
        element.Parent = parent;
        return true;
    }

    [ScriptFunctionDefinition("getElementChildren")]
    public IEnumerable<Element> GetElementChildren(Element element, string? elementType = null)
    {
        IEnumerable<Element> children = element.Children.OfType<Element>();
        if (!string.IsNullOrEmpty(elementType))
            children = children.Where(e => GetElementTypeName(e) == elementType);
        return children;
    }

    [ScriptFunctionDefinition("getElementChild")]
    public Element? GetElementChild(Element element, int index)
        => element.Children.OfType<Element>().ElementAtOrDefault(index);

    [ScriptFunctionDefinition("getElementChildrenCount")]
    public int GetElementChildrenCount(Element element) => element.Children.Count;

    [ScriptFunctionDefinition("getElementCollisionsEnabled")]
    public bool GetElementCollisionsEnabled(Element element) => element.AreCollisionsEnabled;

    [ScriptFunctionDefinition("setElementCollisionsEnabled")]
    public bool SetElementCollisionsEnabled(Element element, bool enabled)
    {
        element.AreCollisionsEnabled = enabled;
        return true;
    }

    [ScriptFunctionDefinition("isElementFrozen")]
    public bool IsElementFrozen(Element element) => element.IsFrozen;

    [ScriptFunctionDefinition("setElementFrozen")]
    public bool SetElementFrozen(Element element, bool freezed)
    {
        element.IsFrozen = freezed;
        return true;
    }

    [ScriptFunctionDefinition("isElementCallPropagationEnabled")]
    public bool IsElementCallPropagationEnabled(Element element) => element.IsCallPropagationEnabled;

    [ScriptFunctionDefinition("setElementCallPropagationEnabled")]
    public bool SetElementCallPropagationEnabled(Element element, bool enabled)
    {
        element.IsCallPropagationEnabled = enabled;
        return true;
    }

    [ScriptFunctionDefinition("isElementDoubleSided")]
    public bool IsElementDoubleSided(Element element) => element is WorldObject obj && obj.DoubleSided;

    [ScriptFunctionDefinition("setElementDoubleSided")]
    public bool SetElementDoubleSided(Element element, bool doubleSided)
    {
        if (element is WorldObject obj)
        {
            obj.DoubleSided = doubleSided;
            return true;
        }
        return false;
    }

    [ScriptFunctionDefinition("isElementLowLOD")]
    public bool IsElementLowLod(Element element) => element is WorldObject obj && obj.IsLowLod;

    [ScriptFunctionDefinition("getLowLODElement")]
    public Element? GetLowLodElement(Element element)
        => element is WorldObject obj ? obj.LowLodElement : null;

    [ScriptFunctionDefinition("setLowLODElement")]
    public bool SetLowLodElement(Element element, WorldObject? lowLod)
    {
        if (element is WorldObject obj)
        {
            obj.LowLodElement = lowLod;
            return true;
        }
        return false;
    }

    [ScriptFunctionDefinition("isElementOnFire")]
    public bool IsElementOnFire(Element element) => element is Ped ped && ped.IsOnFire;

    [ScriptFunctionDefinition("setElementOnFire")]
    public bool SetElementOnFire(Element element, bool onFire)
    {
        if (element is Ped ped)
        {
            ped.IsOnFire = onFire;
            return true;
        }
        return false;
    }

    [ScriptFunctionDefinition("isElementInWater")]
    public bool IsElementInWater(Element element) => element is Ped ped && ped.IsInWater;

    [ScriptFunctionDefinition("isElementWithinMarker")]
    public bool IsElementWithinMarker(Element element, Marker marker)
    {
        var distance = Vector3.Distance(element.Position, marker.Position);
        return distance <= marker.Size;
    }

    [ScriptFunctionDefinition("isElementAttached")]
    public bool IsElementAttached(Element element) => element.Attachment != null;

    [ScriptFunctionDefinition("attachElements")]
    public bool AttachElements(Element theElement, Element theAttachToElement, float xPosOffset = 0, float yPosOffset = 0, float zPosOffset = 0, float xRotOffset = 0, float yRotOffset = 0, float zRotOffset = 0)
    {
        theElement.AttachTo(theAttachToElement, new Vector3(xPosOffset, yPosOffset, zPosOffset), new Vector3(xRotOffset, yRotOffset, zRotOffset));
        return true;
    }

    [ScriptFunctionDefinition("detachElements")]
    public bool DetachElements(Element theElement, Element? theAttachToElement = null)
    {
        theElement.DetachFrom(theAttachToElement);
        return true;
    }

    [ScriptFunctionDefinition("getElementAttachedTo")]
    public Element? GetElementAttachedTo(Element theElement) => theElement.Attachment?.Target;

    [ScriptFunctionDefinition("getAttachedElements")]
    public IEnumerable<Element> GetAttachedElements(Element theElement)
        => theElement.AttachedElements.Select(a => a.Source);

    [ScriptFunctionDefinition("getElementAttachedOffsets")]
    public LuaValue GetElementAttachedOffsets(Element theElement)
    {
        var attachment = theElement.Attachment;
        if (attachment == null)
            return LuaValue.Nil;

        var result = new Dictionary<LuaValue, LuaValue>
        {
            [1] = attachment.PositionOffset.X,
            [2] = attachment.PositionOffset.Y,
            [3] = attachment.PositionOffset.Z,
            [4] = attachment.RotationOffset.X,
            [5] = attachment.RotationOffset.Y,
            [6] = attachment.RotationOffset.Z,
        };
        return new LuaValue(result);
    }

    [ScriptFunctionDefinition("setElementAttachedOffsets")]
    public bool SetElementAttachedOffsets(Element theElement, float xPosOffset = 0, float yPosOffset = 0, float zPosOffset = 0, float xRotOffset = 0, float yRotOffset = 0, float zRotOffset = 0)
    {
        if (theElement.Attachment == null)
            return false;

        theElement.Attachment.PositionOffset = new Vector3(xPosOffset, yPosOffset, zPosOffset);
        theElement.Attachment.RotationOffset = new Vector3(xRotOffset, yRotOffset, zRotOffset);
        return true;
    }

    [ScriptFunctionDefinition("getElementSyncer")]
    public Player? GetElementSyncer(Element element)
        => element is Ped ped ? ped.Syncer : null;

    [ScriptFunctionDefinition("setElementSyncer")]
    public bool SetElementSyncer(Element element, Player? thePlayer)
    {
        if (element is Ped ped)
        {
            ped.Syncer = thePlayer;
            return true;
        }
        return false;
    }

    [ScriptFunctionDefinition("setElementVisibleTo")]
    public bool SetElementVisibleTo(Element theElement, Element visibleTo, bool visible)
    {
        if (visibleTo is RootElement)
        {
            if (visible && !theElement.Associations.Any(a => a.IsGlobal))
                theElement.AssociateWith(server);
            else if (!visible)
                theElement.RemoveFrom(server);
            return true;
        }
        if (visibleTo is Player player)
        {
            if (visible && !theElement.Associations.Any(a => a.Player == player))
                theElement.AssociateWith(player);
            else if (!visible)
                theElement.RemoveFrom(player);
            return true;
        }
        return false;
    }

    [ScriptFunctionDefinition("isElementVisibleTo")]
    public bool IsElementVisibleTo(Element theElement, Element visibleTo)
    {
        if (visibleTo is RootElement)
            return theElement.Associations.Any(a => a.IsGlobal);
        if (visibleTo is Player player)
            return theElement.Associations.Any(a => a.Player == player);
        return false;
    }

    [ScriptFunctionDefinition("clearElementVisibleTo")]
    public bool ClearElementVisibleTo(Element theElement)
    {
        foreach (var association in theElement.Associations.ToArray())
        {
            if (association.Player != null)
                theElement.RemoveFrom(association.Player);
            else if (association.Server != null)
                theElement.RemoveFrom(association.Server);
        }
        theElement.AssociateWith(server);
        return true;
    }

    [ScriptFunctionDefinition("getElementZoneName")]
    public string GetElementZoneName(Element theElement, bool citiesOnly = false) => string.Empty;


    private IEnumerable<Element> GetElementsByTypeName(string typeName)
    {
        return typeName switch
        {
            "player" => elementCollection.GetByType<Player>(),
            "vehicle" => elementCollection.GetByType<Vehicle>(),
            "object" => elementCollection.GetByType<WorldObject>(),
            "ped" => elementCollection.GetByType<Ped>().Where(p => p is not Player),
            "marker" => elementCollection.GetByType<Marker>(),
            "blip" => elementCollection.GetByType<Blip>(),
            "colshape" => elementCollection.GetByType<CollisionShape>(),
            "radararea" => elementCollection.GetByType<RadarArea>(),
            "pickup" => elementCollection.GetByType<Pickup>(),
            "team" => elementCollection.GetByType<Team>(),
            "dummy" => elementCollection.GetAll().OfType<DummyElement>(),
            _ => elementCollection.GetAll().Where(e => GetElementTypeName(e) == typeName)
        };
    }

    private static string GetElementTypeName(Element element) => element switch
    {
        Player => "player",
        DummyElement dummy => dummy.ElementTypeName,
        Ped => "ped",
        Vehicle => "vehicle",
        WorldObject => "object",
        Marker => "marker",
        Blip => "blip",
        CollisionShape => "colshape",
        RadarArea => "radararea",
        Pickup => "pickup",
        Team => "team",
        RootElement => "root",
        _ => element.ElementType.ToString().ToLower()
    };
}

