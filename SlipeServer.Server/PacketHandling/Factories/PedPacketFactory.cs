using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Server.Elements;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Factories
{
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
    }
}
