using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class SetPedRotationRpcPacket : Packet
    { 
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public byte TimeContext { get; }
        public float Rotation { get; set; }
        public bool IsNewWay { get; }

        public SetPedRotationRpcPacket()
        {

        }

        public SetPedRotationRpcPacket(uint elementId, byte timeContext, float rotation, bool isNewWay = true)
        {
            this.ElementId = elementId;
            this.TimeContext = timeContext;
            this.Rotation = rotation;
            this.IsNewWay = isNewWay;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write((byte)ElementRpcFunction.SET_PED_ROTATION);
            builder.WriteElementId(this.ElementId);

            builder.WriteFloatFromBits(this.Rotation, 16, -MathF.PI, MathF.PI, false);
            builder.Write(this.TimeContext);
            builder.Write(this.IsNewWay ? 1 : 0);

            return builder.Build();
        }
    }
}
