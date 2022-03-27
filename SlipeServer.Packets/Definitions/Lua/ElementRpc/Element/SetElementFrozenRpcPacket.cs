using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using System;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class SetElementFrozenRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_LUA_ELEMENT_RPC;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public bool IsFrozen { get; set; }

        public SetElementFrozenRpcPacket()
        {

        }

        public SetElementFrozenRpcPacket(uint elementId, bool isFrozen)
        {
            this.ElementId = elementId;
            this.IsFrozen = isFrozen;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.Write((byte)ElementRpcFunction.SET_ELEMENT_FROZEN);
            builder.WriteElementId(this.ElementId);

            builder.Write(this.IsFrozen);

            return builder.Build();
        }
    }
}
