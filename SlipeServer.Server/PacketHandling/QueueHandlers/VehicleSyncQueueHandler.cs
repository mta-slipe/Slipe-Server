using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class VehicleSyncQueueHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        private readonly IElementRepository elementRepository;
        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] 
        { 
            PacketId.PACKET_ID_PLAYER_VEHICLE_PURESYNC 
        };

        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_PLAYER_VEHICLE_PURESYNC] = typeof(VehiclePureSyncPacket),
        };

        public VehicleSyncQueueHandler(
            ILogger logger,
            IElementRepository elementRepository, 
            int sleepInterval, 
            int workerCount
        ): base(sleepInterval, workerCount)
        {
            this.logger = logger;
            this.elementRepository = elementRepository;
        }

        protected override void HandlePacket(Client client, Packet packet)
        {
            try
            { 
                switch (packet)
                {
                    case VehiclePureSyncPacket vehiclePureSyncPacket:
                        HandleVehiclePureSyncPacket(client, vehiclePureSyncPacket);
                        break;
                }
            } catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
        }

        private void HandleVehiclePureSyncPacket(Client client, VehiclePureSyncPacket packet)
        {
            client.SendPacket(new ReturnSyncPacket(packet.Position));

            packet.PlayerId = client.Player.Id;
            packet.Latency = (ushort)client.Ping;

            var otherPlayers = this.elementRepository
                .GetByType<Player>(ElementType.Player)
                .Where(p => p.Client != client);
            packet.SendTo(otherPlayers);

            var player = client.Player;
            player.RunAsSync(() =>
            {
                player.Position = packet.Position;
                player.Velocity = packet.Velocity;
                player.Health = packet.PlayerHealth;
                player.Armor = packet.PlayerArmor;

                if (packet.AimOrigin != null)
                    player.AimOrigin = packet.AimOrigin.Value;
                if (packet.AimDirection != null)
                    player.AimDirection = packet.AimDirection.Value;


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

                    if (packet.TurretRotation.HasValue)
                        vehicle.TurretRotation = packet.TurretRotation;

                    if (packet.AdjustableProperty.HasValue)
                        vehicle.AdjustableProperty = packet.AdjustableProperty;
                });
            }
        }
    }
}
