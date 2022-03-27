using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.Repositories;
using System;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class VehicleDamageSyncPacketHandler : IPacketHandler<VehicleDamageSyncPacket>
{
    private readonly ISyncHandlerMiddleware<VehicleDamageSyncPacket> middleware;
    private readonly IElementRepository elementRepository;

    public PacketId PacketId => PacketId.PACKET_ID_VEHICLE_DAMAGE_SYNC;

    public VehicleDamageSyncPacketHandler(
        ISyncHandlerMiddleware<VehicleDamageSyncPacket> middleware,
        IElementRepository elementRepository
    )
    {
        this.middleware = middleware;
        this.elementRepository = elementRepository;
    }

    public void HandlePacket(Client client, VehicleDamageSyncPacket packet)
    {
        var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);

        var player = client.Player;

        var vehicle = this.elementRepository.Get(packet.VehicleId) as Elements.Vehicle;

        if (vehicle != null)
        {
            vehicle.RunAsSync(() =>
            {
                foreach (var door in Enum.GetValues<VehicleDoor>())
                    if (packet.DoorStates[(int)door] != null)
                        vehicle.SetDoorState(door, (VehicleDoorState)packet.DoorStates[(int)door]!);

                foreach (var wheel in Enum.GetValues<VehicleWheel>())
                    if (packet.WheelStates[(int)wheel] != null)
                        vehicle.SetWheelState(wheel, (VehicleWheelState)packet.WheelStates[(int)wheel]!);

                foreach (var panel in Enum.GetValues<VehiclePanel>())
                    if (packet.PanelStates[(int)panel] != null)
                        vehicle.SetPanelState(panel, (VehiclePanelState)packet.PanelStates[(int)panel]!);

                foreach (var light in Enum.GetValues<VehicleLight>())
                    if (packet.LightStates[(int)light] != null)
                        vehicle.SetLightState(light, (VehicleLightState)packet.LightStates[(int)light]!);
            });
        }
    }
}
