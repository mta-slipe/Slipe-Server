using SlipeServer.Packets.Definitions.Lua.ElementRpc.Blips;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class BlipPacketFactory
{
    public static DestroyAllBlipsRpcPacket CreateDestroyAllPacket()
    {
        return new DestroyAllBlipsRpcPacket();
    }
}
