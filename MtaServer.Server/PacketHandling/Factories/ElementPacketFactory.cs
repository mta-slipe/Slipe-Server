using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Definitions.Lua.ElementRpc.Element;
using MtaServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace MtaServer.Server.PacketHandling.Factories
{
    public static class ElementPacketFactory
    {
        public static SetElementPositionRpcPacket CreateSetPositionPacket(Element element, Vector3 position, bool isWarp = false)
        {
            return new SetElementPositionRpcPacket(element.Id, element.GetAndIncrementTimeContext(), position, isWarp);
        }
    }
}
