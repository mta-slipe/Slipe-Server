using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;
using System;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class VehicleDamageSyncPacketHandler : IPacketHandler<VehicleDamageSyncPacket>
{
    private readonly ISyncHandlerMiddleware<VehicleDamageSyncPacket> middleware;
    private readonly IElementCollection elementCollection;

    public PacketId PacketId => PacketId.PACKET_ID_VEHICLE_DAMAGE_SYNC;
    
    private readonly int MAX_DOORS = 6;
    private readonly int MAX_WHEELS = 4;
    private readonly int MAX_PANELS = 6;
    private readonly int MAX_LIGHTS = 16;

    public VehicleDamageSyncPacketHandler(
        ISyncHandlerMiddleware<VehicleDamageSyncPacket> middleware,
        IElementCollection elementCollection
    )
    {
        this.middleware = middleware;
        this.elementCollection = elementCollection;
    }

    public void HandlePacket(IClient client, VehicleDamageSyncPacket packet)
    {
        var player = client.Player;
        bool isChanged = false;

        var vehicle = this.elementCollection.Get(packet.VehicleId) as Elements.Vehicle;

        if (vehicle != null)
        {
            vehicle.RunAsSync(() =>
            {
                foreach (var door in Enum.GetValues<VehicleDoor>())
                    if (packet.DoorStates[(int)door] != null)
                    {
                        vehicle.SetDoorState(door, (VehicleDoorState)packet.DoorStates[(int)door]!);
                        isChanged = true;
                    }

                foreach (var wheel in Enum.GetValues<VehicleWheel>())
                    if (packet.WheelStates[(int)wheel] != null)
                    {
                        vehicle.SetWheelState(wheel, (VehicleWheelState)packet.WheelStates[(int)wheel]!);
                        isChanged = true;
                    }

                foreach (var panel in Enum.GetValues<VehiclePanel>())
                    if (packet.PanelStates[(int)panel] != null)
                    {
                        vehicle.SetPanelState(panel, (VehiclePanelState)packet.PanelStates[(int)panel]!);
                        isChanged = true;
                    }

                foreach (var light in Enum.GetValues<VehicleLight>())
                    if (packet.LightStates[(int)light] != null)
                    {
                        vehicle.SetLightState(light, (VehicleLightState)packet.LightStates[(int)light]!);
                        isChanged = true;
                    }
            });
            if (isChanged)
            {
                var syncPacket = new VehicleDamageSyncPacket() { VehicleId = vehicle.Id, };
                for (int i = 0; i < this.MAX_DOORS; i++) syncPacket.DoorStates[i] = vehicle.Damage.Doors[i];
                for (int i = 0; i < this.MAX_WHEELS; i++) syncPacket.WheelStates[i] = vehicle.Damage.Wheels[i];
                for (int i = 0; i< this.MAX_PANELS; i++) syncPacket.PanelStates[i] = vehicle.Damage.Panels[i];
                for (int i = 0; i< this.MAX_LIGHTS; i++) syncPacket.LightStates[i] = vehicle.Damage.Lights[i];
                var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, syncPacket);
                syncPacket.SendTo(otherPlayers);
            }
        }
    }
}
