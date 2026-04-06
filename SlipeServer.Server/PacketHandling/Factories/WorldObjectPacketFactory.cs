using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.WorldObject;
using SlipeServer.Packets.Definitions.Lua.Rpc.Destroys;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class WorldObjectPacketFactory
{
    public static SetElementModelRpcPacket CreateSetModelPacket(WorldObject worldObject)
    {
        return new SetElementModelRpcPacket(worldObject.Id, worldObject.Model);
    }

    public static SetWorldObjectScaleRpcPacket CreateSetScalePacket(WorldObject worldObject)
    {
        return new SetWorldObjectScaleRpcPacket(worldObject.Id, worldObject.Scale);
    }

    public static DestroyAllWorldObjectsRpcPacket CreateDestroyAllPacket()
    {
        return DestroyAllWorldObjectsRpcPacket.Instance;
    }

    public static SetWorldObjectVisibileInAllDimensionsPacket CreateSetVisibleInAllDimensionsPacket(WorldObject worldObject)
    {
        return new SetWorldObjectVisibileInAllDimensionsPacket(worldObject.Id, worldObject.IsVisibleInAllDimensions);
    }

    public static SetWorldObjectLowLodElementRpcPacket CreateSetLowLodElementPacket(WorldObject worldObject)
    {
        return new SetWorldObjectLowLodElementRpcPacket(worldObject.Id, worldObject.LowLodElement?.Id);
    }

    public static SetWorldObjectDoubleSidedRpcPacket CreateSetDoubleSidedPacket(WorldObject worldObject)
    {
        return new SetWorldObjectDoubleSidedRpcPacket(worldObject.Id, worldObject.DoubleSided);
    }

    public static SetWorldObjectIsBreakableRpcPacket CreateSetIsBreakablePacket(WorldObject worldObject)
    {
        return new SetWorldObjectIsBreakableRpcPacket(worldObject.Id, worldObject.IsBreakable);
    }

    public static SetWorldObjectIsBrokenRpcPacket CreateSetIsBrokenPacket(WorldObject worldObject)
    {
        return new SetWorldObjectIsBrokenRpcPacket(worldObject.Id, worldObject.IsBroken);
    }

    public static SetWorldObjectIsRespawnableRpcPacket CreateSetIsRespawnablePacket(WorldObject worldObject)
    {
        return new SetWorldObjectIsRespawnableRpcPacket(worldObject.Id, worldObject.IsRespawnable);
    }
}
