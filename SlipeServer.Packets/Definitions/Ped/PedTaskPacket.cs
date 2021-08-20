using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Definitions.Ped
{
    public class PedTaskPacket : Packet
    {
        public override PacketId PacketId { get; } = PacketId.PACKET_ID_PED_TASK;
        public override PacketReliability Reliability { get; } = PacketReliability.Reliable;
        public override PacketPriority Priority { get; } = PacketPriority.High;

        public uint SourceElementId { get; set; }
        public uint NumberOfBitsInPacketBody { get; set; }
        public bool[] DataBuffer { get; set; }

        public PedTaskPacket(uint sourceElementId)
        {
            this.SourceElementId = sourceElementId;
            this.DataBuffer = new bool[56];
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.SourceElementId);

            builder.Write(this.DataBuffer);
            
            return builder.Build();
        }
        
        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.NumberOfBitsInPacketBody = (uint)(reader.Size / 8);

            uint numBytes = (this.NumberOfBitsInPacketBody + 1) / 8;
            if (numBytes < DataBuffer.Length)
            {
                DataBuffer = reader.GetBits((int)this.NumberOfBitsInPacketBody);
            }
        }
    }
}
