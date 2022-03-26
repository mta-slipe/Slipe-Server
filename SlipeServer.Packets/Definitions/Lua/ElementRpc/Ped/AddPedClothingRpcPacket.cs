using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System;
using System.Numerics;

namespace SlipeServer.Packets.Definitions.Lua.ElementRpc.Ped
{
    public class AddPedClothingRpcPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PLAYER_CLOTHES;

        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public PedClothing[] Clothings { get; set; }

        public AddPedClothingRpcPacket(uint elementId, PedClothing[] clothing)
        {
            this.ElementId = elementId;
            this.Clothings = clothing;
        }

        public override void Read(byte[] bytes)
        {
            throw new NotSupportedException();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.WriteElementId(this.ElementId);
            builder.Write((ushort)this.Clothings.Length);
            foreach (var item in this.Clothings)
            {
                builder.WriteStringWithByteAsLength(item.Texture);
                builder.WriteStringWithByteAsLength(item.Model);
                builder.Write(item.Type);
            }

            return builder.Build();
        }
    }
}
