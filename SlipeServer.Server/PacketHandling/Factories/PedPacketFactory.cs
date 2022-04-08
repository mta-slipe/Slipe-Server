using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Factories;

public static class PedPacketFactory
{
    public static SetElementModelRpcPacket CreateSetModelPacket(Ped ped)
    {
        return new SetElementModelRpcPacket(ped.Id, ped.Model);
    }

    public static SetElementHealthRpcPacket CreateSetHealthPacket(Ped ped)
    {
        return new SetElementHealthRpcPacket(ped.Id, ped.GetAndIncrementTimeContext(), ped.Health);
    }

    public static SetPedArmourRpcPacket CreateSetArmourPacket(Ped ped)
    {
        return new SetPedArmourRpcPacket(ped.Id, ped.GetAndIncrementTimeContext(), ped.Armor);
    }

    public static GivePedJetpackRpcPacket CreateGiveJetpack(Ped ped)
    {
        return new GivePedJetpackRpcPacket(ped.Id);
    }

    public static RemovePedJetpackRpcPacket CreateRemoveJetpack(Ped ped)
    {
        return new RemovePedJetpackRpcPacket(ped.Id);
    }

    public static PlayerStatsPacket CreatePlayerStatsPacket(Ped ped)
    {
        return new PlayerStatsPacket()
        {
            ElementId = ped.Id,
            Stats = ped.GetAllStats().ToDictionary(x => (ushort)x.Key, x => x.Value)
        };
    }

    public static PedClothesPacket CreateClothesPacket(Ped ped, ClothingType cloth, byte index)
    {
        return new PedClothesPacket(ped.Id, new PedClothing[] { ClothesConstants.ClothesTextureModel[cloth][index] });
    }

    public static PedClothesPacket CreateFullClothesPacket(Ped ped)
    {
        return new PedClothesPacket(ped.Id, ped.Clothing.GetClothing().ToArray());
    }
}
