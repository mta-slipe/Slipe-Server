using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using System;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class VehiclePureSyncPacketHandler(
    ISyncHandlerMiddleware<VehiclePureSyncPacket> middleware,
    IElementCollection elementCollection,
    ILogger logger,
    Configuration configuration
    ) : IPacketHandler<VehiclePureSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_VEHICLE_PURESYNC;

    public void HandlePacket(IClient client, VehiclePureSyncPacket packet)
    {
        if (!IsTimeSyncContextValid(client, packet.TimeContext))
            return;

        var player = client.Player;
        var vehicle = player.Vehicle;

        client.SendPacket(new ReturnSyncPacket(packet.Position, packet.Rotation));

        packet.PlayerId = client.Player.Id;
        packet.Latency = (ushort)client.Ping;

        packet.DoorStates = vehicle?.Damage.Doors ?? [];
        packet.WheelStates = vehicle?.Damage.Wheels ?? [];
        packet.PanelStates = vehicle?.Damage.Panels ?? [];
        packet.LightStates = vehicle?.Damage.Lights ?? [];

        var otherPlayers = middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);

        player.RunAsSync(() =>
        {
            var previousHealth = player.Health;
            var previousArmor = player.Armor;

            player.Position = packet.Position;
            player.Velocity = packet.Velocity;
            player.Health = packet.PlayerHealth;
            player.Armor = packet.PlayerArmor;

            player.AimOrigin = packet.AimOrigin;
            player.AimDirection = packet.AimDirection;

            if (packet.WeaponSlot != null)
                player.CurrentWeaponSlot = (WeaponSlot)packet.WeaponSlot;

            var weaponChanged = player.CurrentWeaponSlot != (WeaponSlot?)packet.WeaponSlot;
            if (player.CurrentWeapon != null && packet.WeaponAmmo != null && packet.WeaponAmmoInClip != null)
            {
                var ammoChanged = player.CurrentWeapon.Ammo != packet.WeaponAmmo.Value || player.CurrentWeapon.AmmoInClip != packet.WeaponAmmoInClip.Value;
                player.CurrentWeapon.Ammo = packet.WeaponAmmo.Value;
                player.CurrentWeapon.AmmoInClip = packet.WeaponAmmoInClip.Value;

                if (weaponChanged || ammoChanged)
                    player.TriggerWeaponAmmoUpdate(player.CurrentWeapon.Type, packet.WeaponAmmo.Value, packet.WeaponAmmoInClip.Value);
            }

            player.IsInWater = packet.VehiclePureSyncFlags.IsInWater;
            player.WearsGoggles = packet.VehiclePureSyncFlags.IsWearingGoggles;

            if (packet.DamagerId != null)
            {
                var damager = elementCollection.Get(packet.DamagerId.Value);
                var loss = (previousHealth - packet.PlayerHealth) + (previousArmor - packet.PlayerArmor);

                player.TriggerDamaged(
                    damager,
                    (DamageType)(packet.DamageWeaponType ?? (byte?)DamageType.WEAPONTYPE_UNIDENTIFIED),
                    (BodyPart)(packet.DamageBodyPart ?? (byte?)BodyPart.Torso),
                    loss);
            }

            player.LastMovedUtc = DateTime.UtcNow;
        });

        if (vehicle != null && player == vehicle.Driver)
        {
            vehicle.RunAsSync(() =>
            {
                vehicle.Position = packet.Position;
                vehicle.Rotation = packet.Rotation;
                vehicle.Health = packet.Health;
                vehicle.Velocity = packet.Velocity;
                vehicle.TurnVelocity = packet.TurnVelocity;
                vehicle.TurretRotation = packet.TurretRotation;
                vehicle.AdjustableProperty = packet.AdjustableProperty;
                vehicle.IsSirenActive = packet.VehiclePureSyncFlags.IsSirenOrAlarmActive;

                vehicle.DoorRatios = packet.DoorOpenRatios;

                if (packet.HasTrailer)
                {
                    var previous = vehicle;
                    foreach (var trailer in packet.Trailers)
                    {
                        var trailerElement = elementCollection.Get(trailer.Id) as Elements.Vehicle;
                        if (trailerElement == null)
                            break;

                        trailerElement.RunAsSync(() =>
                        {
                            trailerElement.AttachToTower(previous, true);
                            trailerElement.Position = trailer.Position;
                            trailerElement.Rotation = trailer.Rotation;
                        });
                        previous = trailerElement;
                    }
                } else if (vehicle.TowedVehicle != null)
                {
                    vehicle.TowedVehicle.AttachToTower(null, true);
                }
            });
        }
    }

    private bool IsTimeSyncContextValid(IClient client, byte context)
    {
        if (context != client.Player.TimeContext && context > 0 && client.Player.TimeContext > 0)
        {
            lock (client.Player.TimeContextFailureCountLock)
            {
                client.Player.TimeContextFailureCount++;

                if (client.Player.TimeContextFailureCount > 20)
                {
                    client.Player.TimeContextFailureCount = 0;
                    logger.LogError("Received mismatching Vehicle Pure sync packet from {player} many times, local: {local}, remote: {remote}", client.Player.Name, client.Player.TimeContext, context);

                    if (configuration.Debug.AutoResolveTimeSyncContextMismatches)
                        client.Player.OverrideTimeContext(context);
                }
            }
            logger.LogTrace("Received mismatching Vehicle Pure sync packet from {player}, local: {local}, remote: {remote}", client.Player.Name, client.Player.TimeContext, context);
            return false;
        }

        lock (client.Player.TimeContextFailureCountLock)
        {
            client.Player.TimeContextFailureCount = 0;
        }

        return true;
    }
}
