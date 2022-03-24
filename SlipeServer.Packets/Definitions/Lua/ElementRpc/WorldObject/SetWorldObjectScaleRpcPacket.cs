using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.WorldObject
{
    public class SetWorldObjectScaleRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;

        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; }
        public Vector3 Scale { get; }

        public SetWorldObjectScaleRpcPacket(uint elementId, Vector3 scale)
        {
            this.ElementId = elementId;
            this.Scale = scale;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.Write((byte)ElementRpcFunction.SET_OBJECT_SCALE);
            builder.WriteElementId(this.ElementId);
            builder.Write(this.Scale);

            return builder.Build();
        }
    }
}
