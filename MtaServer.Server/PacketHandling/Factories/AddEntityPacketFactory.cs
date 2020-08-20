using MtaServer.Packets.Definitions.Lua.ElementRpc.Element;
using MtaServer.Server.Elements;
using MtaServer.Server.Elements.ColShapes;
using MtaServer.Server.PacketHandling.Builders;
using System;

namespace MtaServer.Server.PacketHandling.Factories
{
    public static class AddEntityPacketFactory
    {
        public static AddEntityPacket CreateAddEntityPacket(Element[] elements)
        {
            var builder = new AddEntityPacketBuilder();

            foreach(var element in elements)
            {
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
                        if (element is Ped ped)
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
                        if (element is Weapon weapon)
                            builder.AddWeapon(weapon);
                        break;
                }
            }

            return builder.Build();
        }
    }
}
