using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync
{
    public class VehiclePushSyncPacketHandler : IPacketHandler<VehiclePushSyncPacket>
    {
        private readonly IElementRepository elementRepository;

        public PacketId PacketId => PacketId.PACKET_ID_VEHICLE_PUSH_SYNC;

        public VehiclePushSyncPacketHandler(
            IElementRepository elementRepository
        )
        {
            this.elementRepository = elementRepository;
        }

        public void HandlePacket(Client client, VehiclePushSyncPacket packet)
        {
            if (this.elementRepository.Get(packet.ElementId) is Elements.Vehicle vehicle)
            {
                vehicle.TriggerPushed(client.Player);
            }
        }
    }
}
