using MtaServer.Packets.Builder;
using MtaServer.Packets.Enums;
using MtaServer.Packets.Reader;

namespace MtaServer.Packets.Definitions.Sync
{
    public class UnoccupiedVehiclePushSyncPacket : Packet
    {
        public override PacketId PacketId => PacketId.PACKET_ID_VEHICLE_PUSH_SYNC;

        public override PacketFlags Flags => PacketFlags.PACKET_MEDIUM_PRIORITY;

        public uint VehicleId { get; set; }

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
