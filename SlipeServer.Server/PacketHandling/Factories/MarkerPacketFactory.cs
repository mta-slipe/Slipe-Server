using SlipeServer.Packets.Definitions.Lua.Rpc.Destroys;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class MarkerPacketFactory
{
    public static DestroyAllMarkersRpcPacket CreateDestroyAllPacket()
    {
        return DestroyAllMarkersRpcPacket.Instance;
    }
}
