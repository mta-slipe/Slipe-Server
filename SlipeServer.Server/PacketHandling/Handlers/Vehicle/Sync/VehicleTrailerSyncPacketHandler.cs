using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;
using System;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class VehicleTrailerSyncPacketHandler : IPacketHandler<VehicleTrailerSyncPacket>
{
    private readonly ISyncHandlerMiddleware<VehicleTrailerSyncPacket> middleware;
    private readonly IElementCollection elementRepository;

    public PacketId PacketId => PacketId.PACKET_ID_VEHICLE_TRAILER;

    public VehicleTrailerSyncPacketHandler(
        ISyncHandlerMiddleware<VehicleTrailerSyncPacket> middleware,
        IElementCollection elementRepository
    )
    {
        this.middleware = middleware;
        this.elementRepository = elementRepository;
    }

    public void HandlePacket(IClient client, VehicleTrailerSyncPacket packet)
    {
        var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);

        var player = client.Player;

        var vehicle = this.elementRepository.Get(packet.VehicleId) as Elements.Vehicle;
        var attachedVehicle = this.elementRepository.Get(packet.VehicleId) as Elements.Vehicle;

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
