using SlipeServer.Packets.Definitions.Lua.Rpc.Destroys;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class BlipPacketFactory
{
    public static DestroyAllBlipsRpcPacket CreateDestroyAllPacket()
    {
        return new DestroyAllBlipsRpcPacket();
    }
}
