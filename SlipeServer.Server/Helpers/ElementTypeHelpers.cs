using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;

namespace SlipeServer.Server.Helpers;
public static class ElementTypeHelpers
{
    public static ElementType GetElementType<TElement>()
    {
        if (typeof(TElement).IsAssignableTo(typeof(WorldObject)))
            return ElementType.Object;

        if (typeof(TElement).IsAssignableTo(typeof(Player)))
            return ElementType.Player;
        if (typeof(TElement).IsAssignableTo(typeof(Ped)))
            return ElementType.Ped;

        if (typeof(TElement).IsAssignableTo(typeof(Vehicle)))
            return ElementType.Vehicle;

        if (typeof(TElement).IsAssignableTo(typeof(Marker)))
            return ElementType.Marker;
        if (typeof(TElement).IsAssignableTo(typeof(CollisionShape)))
            return ElementType.Colshape;
        if (typeof(TElement).IsAssignableTo(typeof(Blip)))
            return ElementType.Blip;
        if (typeof(TElement).IsAssignableTo(typeof(Pickup)))
            return ElementType.Pickup;
        if (typeof(TElement).IsAssignableTo(typeof(RadarArea)))
            return ElementType.RadarArea;
        if (typeof(TElement).IsAssignableTo(typeof(Team)))
            return ElementType.Team;
        if (typeof(TElement).IsAssignableTo(typeof(Water)))
            return ElementType.Water;
        if (typeof(TElement).IsAssignableTo(typeof(WeaponObject)))
            return ElementType.Weapon;
        if (typeof(TElement).IsAssignableTo(typeof(RootElement)))
            return ElementType.Root;

        return ElementType.Unknown;
    }
}
