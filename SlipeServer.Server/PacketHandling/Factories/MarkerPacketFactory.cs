using SlipeServer.Packets.Definitions.Lua.ElementRpc.Marker;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class MarkerPacketFactory
{
    public static DestroyAllMarkersRpcPacket CreateDestroyAllPacket()
    {
        return new DestroyAllMarkersRpcPacket();
    }
}
