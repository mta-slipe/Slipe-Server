using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;
using System;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.PacketHandling.Factories;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class VehiclePureSyncPacketHandler : IPacketHandler<VehiclePureSyncPacket>
{
    private readonly ISyncHandlerMiddleware<VehiclePureSyncPacket> middleware;
    private readonly IElementCollection elementCollection;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_VEHICLE_PURESYNC;

    public VehiclePureSyncPacketHandler(
        ISyncHandlerMiddleware<VehiclePureSyncPacket> middleware,
        IElementCollection elementCollection
    )
    {
        this.middleware = middleware;
        this.elementCollection = elementCollection;
    }

    public void HandlePacket(IClient client, VehiclePureSyncPacket packet)
    {
        var player = client.Player;
        var vehicle = player.Vehicle;

        client.SendPacket(SyncPacketFactory.CreateReturnSyncPacket(player));

        packet.PlayerId = client.Player.Id;
        packet.Latency = (ushort)client.Ping;

        packet.DoorStates = vehicle?.Damage.Doors ?? Array.Empty<byte>();
        packet.WheelStates = vehicle?.Damage.Wheels ?? Array.Empty<byte>();
        packet.PanelStates = vehicle?.Damage.Panels ?? Array.Empty<byte>();
        packet.LightStates = vehicle?.Damage.Lights ?? Array.Empty<byte>();

        var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);
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
                var damager = this.elementCollection.Get(packet.DamagerId.Value);
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
                        var trailerElement = this.elementCollection.Get(trailer.Id) as Elements.Vehicle;
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
}
