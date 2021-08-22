using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class SetCollisionPolygonHeightRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public Vector2 Height { get; set; }

        public SetCollisionPolygonHeightRpcPacket(uint elementId, Vector2 height)
        {
            this.ElementId = elementId;
            this.Height = height;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write((byte)ElementRpcFunction.SET_COLPOLYGON_HEIGHT);
            builder.WriteElementId(this.ElementId);

            builder.Write(this.Height);

            return builder.Build();
        }
    }
}
