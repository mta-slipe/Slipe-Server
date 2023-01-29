using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class VehiclePushSyncPacketHandler : IPacketHandler<VehiclePushSyncPacket>
{
    private readonly IElementCollection elementCollection;

    public PacketId PacketId => PacketId.PACKET_ID_VEHICLE_PUSH_SYNC;

    public VehiclePushSyncPacketHandler(
        IElementCollection elementCollection
    )
    {
        this.elementCollection = elementCollection;
    }

    public void HandlePacket(IClient client, VehiclePushSyncPacket packet)
    {
        if (this.elementCollection.Get(packet.ElementId) is Elements.Vehicle vehicle)
        {
            vehicle.TriggerPushed(client.Player);
        }
    }
}
