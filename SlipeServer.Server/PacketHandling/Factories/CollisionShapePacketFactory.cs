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

        public static SetCollisionShapeHeightRpcPacket CreateSetHeight(Element element, Vector2 height)
        {
            return new SetCollisionShapeHeightRpcPacket(element.Id, height);
        }

        public static SetCollisionPolygonPointPosition CreatePointPositionChanged(Element element, uint index, Vector2 position)
        {
            return new SetCollisionPolygonPointPosition(element.Id, index, position);
        }

        public static SetCollisionShapeSizeRpcPacket CreateSizeChangedChanged(Element element, Vector3 size)
        {
            return new SetCollisionShapeSizeRpcPacket(element.Id, size);
        }
    }
}
