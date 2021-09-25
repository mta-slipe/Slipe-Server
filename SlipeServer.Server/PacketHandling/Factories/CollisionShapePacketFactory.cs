using SlipeServer.Packets.Definitions.Lua.ElementRpc.CollisionShape;
using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Server.PacketHandling.Factories
{
    public static class CollisionShapePacketFactory
    {
        public static SetCollisionShapeRadiusRpcPacket CreateSetRadius(Element element, float radius)
        {
            return new SetCollisionShapeRadiusRpcPacket(element.Id, radius);
        }

        public static SetCollisionPolygonHeightRpcPacket CreateSetHeight(Element element, Vector2 height)
        {
            return new SetCollisionPolygonHeightRpcPacket(element.Id, height);
        }

        public static SetCollisionPolygonPointPosition CreatePointPositionChanged(Element element, uint index, Vector2 position)
        {
            return new SetCollisionPolygonPointPosition(element.Id, index, position);
        }

        public static SetCollisionShapeSizeRpcPacket CreateSizeChanged(Element element, Vector3 size)
        {
            return new SetCollisionShapeSizeRpcPacket(element.Id, size);
        }

        public static AddCollisionPolygonPointRpcPacket CreatePointAdded(Element element, Vector2 position, int index)
        {
            if(index == -1)
                return new AddCollisionPolygonPointRpcPacket(element.Id, position);
            else
                return new AddCollisionPolygonPointRpcPacket(element.Id, position, (uint)index);
        }

        public static RemoveCollisionPolygonPointRpcPacket CreatePointRemoved(Element element, int index)
        {
            return new RemoveCollisionPolygonPointRpcPacket(element.Id, (uint)index);
        }
    }
}
