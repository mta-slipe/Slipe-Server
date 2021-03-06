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
    }
}
