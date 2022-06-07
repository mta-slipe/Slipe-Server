using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;

public class VehiclePureSyncPacketHandler : IPacketHandler<VehiclePureSyncPacket>
{
    private readonly ISyncHandlerMiddleware<VehiclePureSyncPacket> middleware;
    private readonly IElementCollection elementRepository;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_VEHICLE_PURESYNC;

    public VehiclePureSyncPacketHandler(
        ISyncHandlerMiddleware<VehiclePureSyncPacket> middleware,
        IElementCollection elementRepository
    )
    {
        this.middleware = middleware;
        this.elementRepository = elementRepository;
    }

    public void HandlePacket(IClient client, VehiclePureSyncPacket packet)
    {
        client.SendPacket(new ReturnSyncPacket(packet.Position));

        packet.PlayerId = client.Player.Id;
        packet.Latency = (ushort)client.Ping;

        var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);

        var player = client.Player;
        player.RunAsSync(() =>
        {
            player.Position = packet.Position;
            player.Velocity = packet.Velocity;
            player.Health = packet.PlayerHealth;
            player.Armor = packet.PlayerArmor;

            player.AimOrigin = packet.AimOrigin;
            player.AimDirection = packet.AimDirection;

            if (packet.WeaponSlot != null)
                player.CurrentWeaponSlot = (WeaponSlot)packet.WeaponSlot;

            if (player.CurrentWeapon != null && packet.WeaponAmmo != null && packet.WeaponAmmoInClip != null)
            {
                player.CurrentWeapon.UpdateAmmoCountWithoutTriggerEvent(packet.WeaponAmmo.Value, packet.WeaponAmmoInClip.Value);
            }

            player.IsInWater = packet.VehiclePureSyncFlags.IsInWater;
            player.WearsGoggles = packet.VehiclePureSyncFlags.IsWearingGoggles;

            if (packet.DamagerId != null)
            {
                var damager = this.elementRepository.Get(packet.DamagerId.Value);
                player.TriggerDamaged(damager, (WeaponType)(packet.DamageWeaponType ?? (byte?)WeaponType.WEAPONTYPE_UNIDENTIFIED), (BodyPart)(packet.DamageBodyPart ?? (byte?)BodyPart.Torso));
            }
        });

        var vehicle = player?.Vehicle;

        if (vehicle != null)
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

                vehicle.DoorRatios = packet.DoorOpenRatios;

                if (packet.HasTrailer)
                {
                    var previous = vehicle;
                    foreach (var trailer in packet.Trailers)
                    {
                        var trailerElement = this.elementRepository.Get(trailer.Id) as Elements.Vehicle;
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
