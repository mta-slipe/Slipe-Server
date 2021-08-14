using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SlipeServer.Server.PacketHandling.Factories
{
    public static class ElementPacketFactory
    {
        public static SetElementPositionRpcPacket CreateSetPositionPacket(Element element, Vector3 position, bool isWarp = false)
        {
            return new SetElementPositionRpcPacket(element.Id, element.GetAndIncrementTimeContext(), position, isWarp);
        }

        public static Packet CreateSetRotationPacket(Element element, Vector3 rotation)
        {
            return element switch
            {
                Vehicle => new SetVehicleRotationRpcPacket(element.Id, element.GetAndIncrementTimeContext(), rotation),
                Ped => new SetPedRotationRpcPacket(element.Id, element.GetAndIncrementTimeContext(), rotation.Z * (MathF.PI / 180), true),
                WorldObject => new SetObjectRotationRpcPacket(element.Id, rotation),
                _ => throw new NotImplementedException($"Can not create set rotation packet for {element.GetType()}"),
            };
        }

        public static SetElementHealthRpcPacket CreateSetHealthPacket(Element element, float health)
        {
            return new SetElementHealthRpcPacket(element.Id, element.GetAndIncrementTimeContext(), health);
        }

        public static SetElementAlphaRpcPacket CreateSetAlphaPacket(Element element, byte alpha)
        {
            return new SetElementAlphaRpcPacket(element.Id, element.GetAndIncrementTimeContext(), alpha);
        }
        
        public static TakePlayerScreenshotPacket CreateTakePlayerScreenshotPacket(Element element, ushort sizeX, ushort sizeY, string tag, byte quality, uint maxBandwith, ushort maxPacketSize, Resources.Resource? resource)
        {
            return new TakePlayerScreenshotPacket(element.GetAndIncrementTimeContext(), sizeX, sizeY, tag, quality, maxBandwith, maxPacketSize, resource?.NetId ?? 0);
        }

        public static SetElementDimensionRpcPacket CreateSetDimensionPacket(Element element, ushort dimension)
        {
            return new SetElementDimensionRpcPacket(element.Id, dimension);
        }

        public static SetElementInteriorRpcPacket CreateSetInteriorPacket(Element element, byte interior)
        {
            return new SetElementInteriorRpcPacket(element.Id, interior);
        }
    }
}
