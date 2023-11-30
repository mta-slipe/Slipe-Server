using SlipeServer.Packets.Definitions.Lua.Rpc.Destroys;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class PickupPacketFactory
{
    public static DestroyAllPickupsRpcPacket CreateDestroyAllPacket()
    {
        return DestroyAllPickupsRpcPacket.Instance;
    }
}
