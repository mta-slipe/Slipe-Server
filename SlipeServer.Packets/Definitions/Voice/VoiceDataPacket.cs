using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;
using System.Text;

namespace SlipeServer.Packets.Definitions.Voice
{
    public class VoiceDataPacket : Packet
    {
        public override PacketId PacketId { get; } = PacketId.PACKET_ID_VOICE_DATA;
        public override PacketReliability Reliability { get; } = PacketReliability.ReliableSequenced;
        public override PacketPriority Priority { get; } = PacketPriority.Low;

        public byte[] Buffer { get; set; } = new byte[0];
        public uint SourceElementId { get; set; }

        public VoiceDataPacket(uint sourceElementId, byte[] buffer)
        {
            this.Buffer = buffer;
            this.SourceElementId = sourceElementId;
        }

        public VoiceDataPacket()
        {
            
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.SourceElementId);
            builder.Write((ushort)this.Buffer.Length);
            builder.Write(this.Buffer);

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            ushort length = reader.GetUint16();
            this.Buffer = reader.GetBytes(length);
        }
    }
}