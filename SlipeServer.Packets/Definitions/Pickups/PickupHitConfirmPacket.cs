using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;

namespace SlipeServer.Packets.Definitions.Pickups
{
    public class PickupHitConfirmPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_PICKUP_HIT_CONFIRM;
        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;
        public override PacketPriority Priority => PacketPriority.High;

        public uint ElementId { get; set; }
        public bool IsVisible { get; }
        public bool PlaysSounds { get; }

        public PickupHitConfirmPacket(uint elementId, bool isVisible, bool playsSounds)
        {
            this.ElementId = elementId;
            this.IsVisible = isVisible;
            this.PlaysSounds = playsSounds;
        }

        public PickupHitConfirmPacket()
        {

        }

        public override void Read(byte[] bytes)
        {

        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.ElementId);
            builder.Write(this.IsVisible);
            builder.Write(this.PlaysSounds);

            return builder.Build();
        }
    }
}
