using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Player;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped;
using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Constants;

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

        public static AddPedClothingRpcPacket CreateAddPedClothingPacket(Ped ped, Clothes cloth, byte index)
        {
            return new AddPedClothingRpcPacket(ped.Id, new PedClothing[] { ClothesConstants.ClothesTextureModel[cloth][index] });
        }
    }
}
