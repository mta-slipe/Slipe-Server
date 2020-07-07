using System;
using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;

namespace MtaServer.Packets.Definitions.Lua.ElementRpc.Element
{
    public class RemoveEntityPacket : Packet
    { 
        public override PacketId PacketId => PacketId.PACKET_ID_ENTITY_REMOVE;
        public override PacketFlags Flags => PacketFlags.PACKET_HIGH_PRIORITY | PacketFlags.PACKET_RELIABLE | PacketFlags.PACKET_SEQUENCED;

        private readonly PacketBuilder builder;

        public RemoveEntityPacket()
        {
            this.builder = new PacketBuilder();
        }

        public void AddEntity(uint elementId)
        {
            builder.WriteElementId(elementId);
        }

        public override void Read(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public override byte[] Write()
        {
            builder.Write(builder.Build());

            return builder.Build();
        }
    }
}
