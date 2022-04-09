using SlipeServer.Packets.Definitions.Lua.ElementRpc.RadarArea;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class RadarAreaPacketFactory
{
    public static DestroyAllRadarAreasRpcPacket CreateDestroyAllPacket()
    {
        return new DestroyAllRadarAreasRpcPacket();
    }
}
