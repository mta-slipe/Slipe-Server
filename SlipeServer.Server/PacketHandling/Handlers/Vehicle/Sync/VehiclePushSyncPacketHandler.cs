using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync
{
    public class VehiclePushSyncPacketHandler : IPacketHandler<VehiclePushSyncPacket>
    {
        private readonly ISyncHandlerMiddleware<VehiclePushSyncPacket> middleware;
        private readonly IElementRepository elementRepository;

        public PacketId PacketId => PacketId.PACKET_ID_VEHICLE_PUSH_SYNC;

        public VehiclePushSyncPacketHandler(
            ISyncHandlerMiddleware<VehiclePushSyncPacket> middleware,
            IElementRepository elementRepository
        )
        {
            this.middleware = middleware;
            this.elementRepository = elementRepository;
        }

        public void HandlePacket(Client client, VehiclePushSyncPacket packet)
        {
            var vehicle = this.elementRepository.Get(packet.ElementId) as Elements.Vehicle;

            if (vehicle != null)
            {
                vehicle.TriggerPushed(client.Player);
            }
        }
    }
}
