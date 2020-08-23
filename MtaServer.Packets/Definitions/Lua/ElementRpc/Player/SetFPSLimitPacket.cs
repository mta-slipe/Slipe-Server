using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.ElementRpc.Player
{
    public class SetFPSLimitPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA;
        public override PacketFlags Flags => PacketFlags.PACKET_HIGH_PRIORITY | PacketFlags.PACKET_RELIABLE | PacketFlags.PACKET_SEQUENCED;

        public ushort Limit { get; set; }

        public SetFPSLimitPacket(ushort limit)
        {
            this.Limit = limit;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_FPS_LIMIT);
            builder.Write(Limit);
            return builder.Build();
        }
    }
}
