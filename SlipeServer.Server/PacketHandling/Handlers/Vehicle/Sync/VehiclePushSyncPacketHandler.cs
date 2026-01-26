using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class VehiclePushSyncPacketHandler(
    IElementCollection elementCollection
    ) : IPacketHandler<VehiclePushSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_VEHICLE_PUSH_SYNC;

    public void HandlePacket(IClient client, VehiclePushSyncPacket packet)
    {
        if (elementCollection.Get(packet.ElementId) is Elements.Vehicle vehicle)
        {
            vehicle.TriggerPushed(client.Player);
        }
    }
}
