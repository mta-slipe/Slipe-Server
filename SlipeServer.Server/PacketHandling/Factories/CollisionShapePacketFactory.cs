using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Server.Elements.ColShapes;

namespace SlipeServer.Server.PacketHandling.Factories
{
    public static class CollisionShapePacketFactory
    {
        public static SetCollisionShapeRadiusRpcPacket CreateSetRadius(Element element, float radius)
        {
            return new SetCollisionShapeRadiusRpcPacket(element.Id, radius);
        }
    }
}
