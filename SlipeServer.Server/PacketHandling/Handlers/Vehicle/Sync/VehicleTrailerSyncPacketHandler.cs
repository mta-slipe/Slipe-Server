using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class VehicleTrailerSyncPacketHandler(
    ISyncHandlerMiddleware<VehicleTrailerSyncPacket> middleware,
    IElementCollection elementCollection
    ) : IPacketHandler<VehicleTrailerSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_VEHICLE_TRAILER;

    public void HandlePacket(IClient client, VehicleTrailerSyncPacket packet)
    {
        var otherPlayers = middleware.GetPlayersToSyncTo(client.Player, packet);

        var player = client.Player;

        var vehicle = elementCollection.Get(packet.VehicleId) as Elements.Vehicle;
        var attachedVehicle = elementCollection.Get(packet.VehicleId) as Elements.Vehicle;

        if (vehicle != null && attachedVehicle != null)
        {
            vehicle.RunAsSync(() =>
            {
                if (packet.IsAttached)
                {
                    if (vehicle.TowedVehicle != null)
                        VehiclePacketFactory.CreateTrailerDetachPacket(vehicle, vehicle.TowedVehicle)
                            .SendTo(otherPlayers);
                    if (attachedVehicle.TowingVehicle != null)
                        VehiclePacketFactory.CreateTrailerDetachPacket(attachedVehicle.TowingVehicle, attachedVehicle)
                            .SendTo(otherPlayers);

                    attachedVehicle.RunAsSync(() => attachedVehicle.AttachToTower(vehicle, true));
                } else
                {
                    attachedVehicle.RunAsSync(() => attachedVehicle.AttachToTower(null, true));
                }

                packet.SendTo(otherPlayers);
            });
        }
    }
}
