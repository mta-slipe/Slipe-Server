using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Sync;
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
    public class SyncQueueHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        private readonly IElementRepository elementRepository;
        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] 
        { 
            PacketId.PACKET_ID_CAMERA_SYNC, 
            PacketId.PACKET_ID_PLAYER_KEYSYNC, 
            PacketId.PACKET_ID_PLAYER_PURESYNC 
        };

        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_CAMERA_SYNC] = typeof(CameraSyncPacket),
            [PacketId.PACKET_ID_PLAYER_KEYSYNC] = typeof(KeySyncPacket),
            [PacketId.PACKET_ID_PLAYER_PURESYNC] = typeof(PlayerPureSyncPacket),
        };

        public SyncQueueHandler(
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
                    case CameraSyncPacket cameraSyncPacket:
                        HandleCameraSyncPacket(client, cameraSyncPacket);
                        break;
                    case KeySyncPacket keySyncPacket:
                        HandleClientKeySyncPacket(client, keySyncPacket);
                        break;
                    case PlayerPureSyncPacket playerPureSyncPacket:
                        HandleClientPureSyncPacket(client, playerPureSyncPacket);
                        break;
                }
            } catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
        }

        private void HandleCameraSyncPacket(Client client, CameraSyncPacket packet)
        {
            var player = client.Player;
            player.RunAsSync(() =>
            {
                if (packet.IsFixed)
                {
                    player.Camera.Position = packet.Position;
                    player.Camera.LookAt = packet.LookAt;
                } else
                {
                    player.Camera.Target = this.elementRepository.Get(packet.TargetId);
                }
            });
        }

        private void HandleClientKeySyncPacket(Client client, KeySyncPacket packet)
        {
            packet.PlayerId = client.Player.Id;
            packet.SendTo(this.elementRepository.GetByType<Player>(ElementType.Player).Where(p => p.Client != client));
        }

        private void HandleClientPureSyncPacket(Client client, PlayerPureSyncPacket packet)
        {
            client.SendPacket(new ReturnSyncPacket(packet.Position));

            packet.PlayerId = client.Player.Id;
            packet.Latency = 0;

            var otherPlayers = this.elementRepository
                .GetByType<Player>(ElementType.Player)
                .Where(p => p.Client != client);
            packet.SendTo(otherPlayers);

            var player = client.Player;
            player.RunAsSync(() =>
            {
                player.Position = packet.Position;
                player.Velocity = packet.Velocity;
                player.Health = packet.Health;
                player.Armor = packet.Armor;
                player.AimOrigin = packet.AimOrigin;
                player.AimDirection = packet.AimDirection;

                player.ContactElement = this.elementRepository.Get(packet.ContactElementId);

                player.CurrentWeaponSlot = WeaponConstants.SlotPerWeapon[(WeaponId)packet.WeaponType];
                if (player.CurrentWeapon != null && player.CurrentWeapon.Type == (WeaponId)packet.WeaponType)
                {
                    player.CurrentWeapon.UpdateAmmoCountWithoutTriggerEvent(packet.TotalAmmo, packet.AmmoInClip);
                }

                player.IsInWater = packet.SyncFlags.IsInWater;
                player.IsOnGround = packet.SyncFlags.IsOnGround;
                player.HasJetpack = packet.SyncFlags.HasJetpack;
                player.IsDucked = packet.SyncFlags.IsDucked;
                player.WearsGoggles = packet.SyncFlags.WearsGoggles;
                player.HasContact = packet.SyncFlags.HasContact;
                player.IsChoking = packet.SyncFlags.IsChoking;
                player.AkimboTargetUp = packet.SyncFlags.AkimboTargetUp;
                player.IsOnFire = packet.SyncFlags.IsOnFire;
                player.IsSyncingVelocity = packet.SyncFlags.IsSyncingVelocity;
                player.IsStealthAiming = packet.SyncFlags.IsStealthAiming;

                player.CameraPosition = packet.CameraOrientation.CameraPosition;
                player.CameraDirection = packet.CameraOrientation.CameraForward;
                player.CameraRotation = packet.CameraRotation;

                if (packet.IsDamageChanged)
                {
                    var damager = this.elementRepository.Get(packet.DamagerId);
                    player.TriggerDamaged(damager, (WeaponType)packet.DamageType, (BodyPart)packet.DamageBodypart);
                }
            });
        }
    }
}
