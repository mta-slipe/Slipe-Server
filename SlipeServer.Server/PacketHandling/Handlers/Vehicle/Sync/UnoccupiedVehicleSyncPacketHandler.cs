using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class UnoccupiedVehicleSyncPacketHandler(
    ISyncHandlerMiddleware<UnoccupiedVehicleSyncPacket> middleware,
    IElementCollection elementCollection
    ) : IPacketHandler<UnoccupiedVehicleSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_UNOCCUPIED_VEHICLE_SYNC;

    public void HandlePacket(IClient client, UnoccupiedVehicleSyncPacket packet)
    {
        List<UnoccupiedVehicleSync> vehiclesToSync = new();

        foreach (var vehicle in packet.Vehicles)
        {
            Elements.Vehicle vehicleElement = (Elements.Vehicle)elementCollection.Get(vehicle.Id)!;

            if (vehicleElement != null)
            {
                if (vehicleElement.Syncer?.Client == client && vehicleElement.CanUpdateSync(vehicle.TimeContext))
                {
                    vehicleElement.RunAsSync(() =>
                    {
                        if (vehicle.Position != null)
                            vehicleElement.Position = vehicle.Position.Value;

                        if (vehicle.Rotation != null)
                            vehicleElement.Rotation = vehicle.Rotation.Value;

                        if (vehicle.Velocity != null)
                            vehicleElement.Velocity = vehicle.Velocity.Value;

                        if (vehicle.TurnVelocity != null)
                            vehicleElement.TurnVelocity = vehicle.TurnVelocity.Value;

                        if (vehicle.Health != null)
                            vehicleElement.Health = vehicle.Health.Value;

                        if (vehicle.Trailer != null)
                        {
                            var trailer = elementCollection.Get(vehicle.Trailer.Value) as Elements.Vehicle;
                            if (trailer != null)
                            {
                                vehicleElement.AttachTrailer(trailer, true);
                            }
                        } else if (vehicleElement.TowingVehicle != null)
                        {
                            vehicleElement.AttachTrailer(null, true);
                        }

                        vehicleElement.IsInWater = (vehicle.Flags & UnoccupiedVehicleSyncFlags.IsInWater) > 0;
                        vehicleElement.IsDerailed = (vehicle.Flags & UnoccupiedVehicleSyncFlags.Derailed) > 0;
                        vehicleElement.IsEngineOn = (vehicle.Flags & UnoccupiedVehicleSyncFlags.Engine) > 0;

                    });

                    vehiclesToSync.Add(vehicle);
                }
            }
        }

        var players = middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.Vehicles = vehiclesToSync;
        packet.SendTo(players);
    }
}
