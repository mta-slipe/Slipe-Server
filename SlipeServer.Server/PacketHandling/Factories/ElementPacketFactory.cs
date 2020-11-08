using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
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
            switch (element)
            {
                case Vehicle:
                    return new SetVehicleRotationRpcPacket(element.Id, element.GetAndIncrementTimeContext(), rotation);
                case Ped:
                    return new SetPedRotationRpcPacket(element.Id, element.GetAndIncrementTimeContext(), rotation.Z * (MathF.PI / 180), true);
                case WorldObject:
                    return new SetObjectRotationRpcPacket(element.Id, rotation);
            }
            throw new NotImplementedException($"Can not create set rotation packet for {element.GetType()}");
        }

        public static SetElementHealthRpcPacket CreateSetHealthPacket(Element element, float health)
        {
            return new SetElementHealthRpcPacket(element.Id, element.GetAndIncrementTimeContext(), health);
        }

        public static SetElementAlphaRpcPacket CreateSetAlphaPacket(Element element, byte alpha)
        {
            return new SetElementAlphaRpcPacket(element.Id, element.GetAndIncrementTimeContext(), alpha);
        }
    }
}
