using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.PacketHandling.Builders;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class AddEntityPacketFactory
{
    public static AddEntityPacket CreateAddEntityPacket(IEnumerable<Element> elements)
    {
        var builder = new AddEntityPacketBuilder();

        foreach (var element in elements)
        {
            if (element.Id == 0 && element.ElementType != ElementType.Root)
                throw new System.Exception(string.Format("Element {0} can not be created with id 0", element.ElementType));

            switch (element.ElementType)
            {
                case ElementType.Dummy:
                    if (element is DummyElement dummy)
                        builder.AddDummy(dummy);
                    break;

                case ElementType.Object:
                    if (element is WorldObject obj)
                        builder.AddObject(obj);
                    break;

                case ElementType.Blip:
                    if (element is Blip blip)
                        builder.AddBlip(blip);
                    break;

                case ElementType.Colshape:
                    if (element is CollisionShape colShape)
                        builder.AddColShape(colShape);
                    break;

                case ElementType.Marker:
                    if (element is Marker marker)
                        builder.AddMarker(marker);
                    break;

                case ElementType.Ped:
                    if (element is Ped ped && element is not Player)
                        builder.AddPed(ped);
                    break;

                case ElementType.Pickup:
                    if (element is Pickup pickup)
                        builder.AddPickup(pickup);
                    break;

                case ElementType.RadarArea:
                    if (element is RadarArea radarArea)
                        builder.AddRadarArea(radarArea);
                    break;

                case ElementType.Team:
                    if (element is Team team)
                        builder.AddTeam(team);
                    break;

                case ElementType.Water:
                    if (element is Water water)
                        builder.AddWater(water);
                    break;

                case ElementType.Vehicle:
                    if (element is Vehicle vehicle)
                        builder.AddVehicle(vehicle);
                    break;

                case ElementType.Weapon:
                    if (element is WeaponObject weapon)
                        builder.AddWeapon(weapon);
                    break;
            }
        }

        return builder.Build();
    }
}
