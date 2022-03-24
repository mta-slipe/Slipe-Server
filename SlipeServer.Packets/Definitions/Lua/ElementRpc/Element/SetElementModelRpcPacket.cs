using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class SetElementModelRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public ushort Model { get; set; }
        public byte? Variant1 { get; set; }
        public byte? Variant2 { get; set; }

        public SetElementModelRpcPacket()
        {

        }

        public SetElementModelRpcPacket(uint elementId, ushort model, byte? variant1 = null, byte? variant2 = null)
        {
            this.ElementId = elementId;
            this.Model = model;
            this.Variant1 = variant1;
            this.Variant2 = variant2;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write((byte)ElementRpcFunction.SET_ELEMENT_MODEL);
            builder.WriteElementId(this.ElementId);

            builder.Write(this.Model);

            if (this.Variant1.HasValue)
                builder.Write(this.Variant1.Value);
            if (this.Variant2.HasValue)
                builder.Write(this.Variant2.Value);

            return builder.Build();
        }
    }
}
