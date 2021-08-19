using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Ped
{
    public class PedStopSyncPacket : Packet
    {
        public override PacketId PacketId { get; } = PacketId.PACKET_ID_PED_STOPSYNC;
        public override PacketReliability Reliability { get; } = PacketReliability.ReliableSequenced;
        public override PacketPriority Priority { get; } = PacketPriority.High;

        public uint SourceElementId { get; set; }

        public PedStopSyncPacket(uint sourceElementId)
        {
            this.SourceElementId = sourceElementId;
        }


        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.SourceElementId);

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {
            
        }
    }
}
