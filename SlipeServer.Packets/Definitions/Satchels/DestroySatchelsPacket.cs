using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Satchels
{
    public class DestroySatchelsPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_DESTROY_SATCHELS;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }

        public DestroySatchelsPacket(uint elementId)
        {
            this.ElementId = elementId;
        }

        public DestroySatchelsPacket()
        {

        }

        public override void Read(byte[] bytes)
        {

        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.ElementId);

            return builder.Build();
        }
    }
}
