using SlipeServer.Packets.Definitions.Lua.Rpc.Destroys;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class RadarAreaPacketFactory
{
    public static DestroyAllRadarAreasRpcPacket CreateDestroyAllPacket()
    {
        return new DestroyAllRadarAreasRpcPacket();
    }
}
