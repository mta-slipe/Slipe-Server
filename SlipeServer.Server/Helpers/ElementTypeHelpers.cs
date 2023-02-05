using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using System;
using System.Collections.Concurrent;

namespace SlipeServer.Server.Helpers;

public static class ElementTypeHelpers
{
    private readonly static ConcurrentDictionary<Type, ElementType> elementTypeCache = new();

    private static ElementType DeterineElementType(Type type)
    {
        if (elementTypeCache.TryGetValue(type, out var value))
            return value;


        if (IsClassAssignableTo(type, typeof(WorldObject), ElementType.Object))
            return ElementType.Object;

        if (IsClassAssignableTo(type, typeof(Player), ElementType.Player))
            return ElementType.Player;
        if (IsClassAssignableTo(type, typeof(Ped), ElementType.Ped))
            return ElementType.Ped;

        if (IsClassAssignableTo(type, typeof(Vehicle), ElementType.Vehicle))
            return ElementType.Vehicle;

        if (IsClassAssignableTo(type, typeof(Marker), ElementType.Marker))
            return ElementType.Marker;
        if (IsClassAssignableTo(type, typeof(CollisionShape), ElementType.Colshape))
            return ElementType.Colshape;
        if (IsClassAssignableTo(type, typeof(Blip), ElementType.Blip))
            return ElementType.Blip;
        if (IsClassAssignableTo(type, typeof(Pickup), ElementType.Pickup))
            return ElementType.Pickup;
        if (IsClassAssignableTo(type, typeof(RadarArea), ElementType.RadarArea))
            return ElementType.RadarArea;
        if (IsClassAssignableTo(type, typeof(Team), ElementType.Team))
            return ElementType.Team;
        if (IsClassAssignableTo(type, typeof(Water), ElementType.Water))
            return ElementType.Water;
        if (IsClassAssignableTo(type, typeof(WeaponObject), ElementType.Weapon))
            return ElementType.Weapon;
        if (IsClassAssignableTo(type, typeof(RootElement), ElementType.Root))
            return ElementType.Root;

        return ElementType.Unknown;
    }

    private static bool IsClassAssignableTo(Type requestedType, Type type, ElementType elementType)
    {
        var result = requestedType.IsAssignableTo(type);
        if (result)
            elementTypeCache[requestedType] = elementType;

        return result;
    }

    /// <summary>
    /// Gets the elements type of a specific class, this supports inherited element classes
    /// </summary>
    public static ElementType GetElementType<TElement>()
    {
        return DeterineElementType(typeof(TElement));
    }
}
