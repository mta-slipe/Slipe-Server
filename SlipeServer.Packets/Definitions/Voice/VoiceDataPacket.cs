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

        public byte[]? Buffer { get; set; }
        public uint SourceElementId { get; set; }

        public VoiceDataPacket(uint elementId, byte[] sourceBuffer)
        {
            this.Buffer = sourceBuffer;
            
            this.SourceElementId = elementId;
        }
        
        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            // Write the source player id
            builder.WriteElementId(this.SourceElementId);
            // Write the length as an unsigned short integer
            builder.Write((ushort)this.Buffer!.Length);
            // Write the string data
            builder.Write(Encoding.Default.GetString(this.Buffer));

            return builder.Build();
        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.Buffer = reader.GetBytes(bytes.Length);
        }
    }
}