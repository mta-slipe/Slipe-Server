using SlipeServer.Packets.Definitions.Lua.ElementRpc.Pickups;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class PickupPacketFactory
{
    public static DestroyAllPickupsRpcPacket CreateDestroyAllPacket()
    {
        return new DestroyAllPickupsRpcPacket();
    }
}
