using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;
using System;
using System.Linq;
using SlipeServer.Server.Clients;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class VehicleDamageSyncPacketHandler(
    ISyncHandlerMiddleware<VehicleDamageSyncPacket> middleware,
    IElementCollection elementCollection
    ) : IPacketHandler<VehicleDamageSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_VEHICLE_DAMAGE_SYNC;

    public void HandlePacket(IClient client, VehicleDamageSyncPacket packet)
    {
        var vehicle = elementCollection.Get(packet.VehicleId) as Elements.Vehicle;

        if (vehicle == null)
            return;

        var damage = vehicle.Damage;
        var isChanged =
            IsChanged(packet.DoorStates, damage.Doors) ||
            IsChanged(packet.WheelStates, damage.Wheels) ||
            IsChanged(packet.PanelStates, damage.Panels) ||
            IsChanged(packet.LightStates, damage.Lights);

        if (!isChanged)
            return;

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

        // Relay the server's authoritative damage model instead of the raw values the client sent, so that
        // other players can not end up with a damage model that differs from the server's.
        var syncPacket = CreateSyncPacket(vehicle);
        syncPacket.SendTo(middleware.GetPlayersToSyncTo(client.Player, syncPacket));
    }

    private static bool IsChanged(byte?[] incomingStates, byte[] currentStates)
    {
        for (var i = 0; i < Math.Min(incomingStates.Length, currentStates.Length); i++)
            if (incomingStates[i] is byte state && state != currentStates[i])
                return true;

        return false;
    }

    private static VehicleDamageSyncPacket CreateSyncPacket(Elements.Vehicle vehicle)
    {
        var damage = vehicle.Damage;

        return new VehicleDamageSyncPacket
        {
            VehicleId = vehicle.Id,
            DoorStates = [.. damage.Doors.Select(x => (byte?)x)],
            WheelStates = [.. damage.Wheels.Select(x => (byte?)x)],
            PanelStates = [.. damage.Panels.Select(x => (byte?)x)],
            LightStates = [.. damage.Lights.Select(x => (byte?)x)],
        };
    }
}
