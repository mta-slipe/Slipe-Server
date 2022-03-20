using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Reader;

namespace SlipeServer.Packets.Definitions.Vehicles
{

    public class VehiclePushSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_VEHICLE_PUSH_SYNC;

        public override PacketReliability Reliability => PacketReliability.ReliableSequenced;

        public override PacketPriority Priority => PacketPriority.Medium;

        public uint ElementId { get; set; }


        public VehiclePushSyncPacket()
        {

        }

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);

            this.ElementId = reader.GetElementId();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();

            builder.WriteElementId(this.ElementId);

            return builder.Build();
        }

        public override void Reset()
        {

        }
    }
}
