using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Builder;
using SlipeServer.Packets.Reader;
using SlipeServer.Packets;

namespace SlipeServer.Packets.Definitions.Sync
{
    public class UnoccupiedVehiclePushSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_VEHICLE_PUSH_SYNC;

        public uint VehicleId { get; set; }

        public override PacketReliability Reliability => PacketReliability.UnreliableSequenced;

        public override PacketPriority Priority => PacketPriority.Medium;

        public override void Read(byte[] bytes)
        {
            var reader = new PacketReader(bytes);
            this.VehicleId = reader.GetElementId();
        }

        public override byte[] Write()
        {
            var builder = new PacketBuilder();
            builder.WriteElementId(VehicleId);
            return builder.Build();
        }
    }
}
