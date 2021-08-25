using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.PacketHandling.Factories
{
    public static class WorldObjectPacketFactory
    {
        public static SetElementModelRpcPacket CreateSetModelPacket(WorldObject worldObject)
        {
            return new SetElementModelRpcPacket(worldObject.Id, worldObject.Model);
        }
    }
}
