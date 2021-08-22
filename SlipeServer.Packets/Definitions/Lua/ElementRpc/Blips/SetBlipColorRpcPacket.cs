using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class SetBlipColorRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public Color Color { get; set; }

        public SetBlipColorRpcPacket()
        {

        }

        public SetBlipColorRpcPacket(uint elementId, Color color)
        {
            this.ElementId = elementId;
            this.Color = color;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write((byte)ElementRpcFunction.SET_BLIP_COLOR);
            builder.WriteElementId(this.ElementId);
            builder.Write(this.Color, true);

            return builder.Build();
        }
    }
}
