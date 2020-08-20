using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MtaServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class SetElementAlphaRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketFlags Flags => PacketFlags.PACKET_HIGH_PRIORITY | PacketFlags.PACKET_RELIABLE | PacketFlags.PACKET_SEQUENCED;

        public uint ElementId { get; set; }
        public byte TimeContext { get; set; }
        public byte Alpha { get; set; }

        public SetElementAlphaRpcPacket()
        {

        }

        public SetElementAlphaRpcPacket(uint elementId, byte timeContext, byte alpha)
        {
            this.ElementId = elementId;
            this.TimeContext = timeContext;
            this.Alpha = alpha;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write((byte)ElementRpcFunction.SET_ELEMENT_ALPHA);
            builder.WriteElementId(this.ElementId);

            builder.Write(this.Alpha);

            builder.Write(this.TimeContext);

            return builder.Build();
        }
    }
}
