using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class SyncQueueHandler : OrderedScalingWorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        private readonly IElementRepository elementRepository;
        private readonly ISyncHandlerMiddleware<PlayerPureSyncPacket> pureSyncMiddleware;
        private readonly ISyncHandlerMiddleware<ProjectileSyncPacket> projectileSyncMiddleware;
        private readonly ISyncHandlerMiddleware<KeySyncPacket> keySyncMiddleware;
        private readonly ISyncHandlerMiddleware<PlayerBulletSyncPacket> playerBulletSyncMiddleware;
        private readonly ISyncHandlerMiddleware<WeaponBulletSyncPacket> weaponBulletSyncMiddleware;

        public override IEnumerable<PacketId> SupportedPacketIds => this.PacketTypes.Keys;

        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_CAMERA_SYNC] = typeof(CameraSyncPacket),
            [PacketId.PACKET_ID_PLAYER_KEYSYNC] = typeof(KeySyncPacket),
            [PacketId.PACKET_ID_PLAYER_PURESYNC] = typeof(PlayerPureSyncPacket),
            [PacketId.PACKET_ID_PROJECTILE] = typeof(ProjectileSyncPacket),
            [PacketId.PACKET_ID_PLAYER_BULLETSYNC] = typeof(PlayerBulletSyncPacket),
            [PacketId.PACKET_ID_WEAPON_BULLETSYNC] = typeof(WeaponBulletSyncPacket),
        };

        public SyncQueueHandler(
            ILogger logger,
            IElementRepository elementRepository,
            ISyncHandlerMiddleware<PlayerPureSyncPacket> pureSyncMiddleware,
            ISyncHandlerMiddleware<ProjectileSyncPacket> projectileSyncMiddleware,
            ISyncHandlerMiddleware<KeySyncPacket> keySyncMiddleware,
            ISyncHandlerMiddleware<PlayerBulletSyncPacket> playerBulletSyncMiddleware,
            ISyncHandlerMiddleware<WeaponBulletSyncPacket> weaponBulletSyncMiddleware,
            int sleepInterval,
            QueueHandlerScalingConfig config
        ) : base(config, sleepInterval)
        {
            this.logger = logger;
            this.elementRepository = elementRepository;
            this.pureSyncMiddleware = pureSyncMiddleware;
            this.projectileSyncMiddleware = projectileSyncMiddleware;
            this.keySyncMiddleware = keySyncMiddleware;
            this.playerBulletSyncMiddleware = playerBulletSyncMiddleware;
            this.weaponBulletSyncMiddleware = weaponBulletSyncMiddleware;
        }

        protected override Task HandlePacket(Client client, Packet packet)
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
                    case ProjectileSyncPacket projectileSyncPacket:
                        HandleProjectileSyncPacket(client, projectileSyncPacket);
                        break;
                    case PlayerBulletSyncPacket playerBulletSyncPacket:
                        HandlePlayerBulletSyncPacket(client, playerBulletSyncPacket);
                        break;
                    case WeaponBulletSyncPacket weaponBulletSyncPacket:
                        HandleWeaponBulletSyncPacket(client, weaponBulletSyncPacket);
                        break;
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
            return Task.CompletedTask;
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
            var otherPlayers = this.keySyncMiddleware.GetPlayersToSyncTo(client.Player, packet);
            packet.SendTo(otherPlayers);
        }

        private void HandleProjectileSyncPacket(Client client, ProjectileSyncPacket packet)
        {
            var otherPlayers = this.projectileSyncMiddleware.GetPlayersToSyncTo(client.Player, packet);
            packet.SendTo(otherPlayers);
        }

        private void HandleClientPureSyncPacket(Client client, PlayerPureSyncPacket packet)
        {
            client.SendPacket(new ReturnSyncPacket(packet.Position));
            packet.PlayerId = client.Player.Id;
            packet.Latency = (ushort)client.Ping;

            var otherPlayers = this.pureSyncMiddleware.GetPlayersToSyncTo(client.Player, packet);
            if (otherPlayers.Any())
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

            player.TriggerSync();
        }

        private void HandlePlayerBulletSyncPacket(Client client, PlayerBulletSyncPacket packet)
        {
            packet.SourceElementId = client.Player.Id;
            var otherPlayers = this.playerBulletSyncMiddleware.GetPlayersToSyncTo(client.Player, packet);
            packet.SendTo(otherPlayers);
        }

        private void HandleWeaponBulletSyncPacket(Client client, WeaponBulletSyncPacket packet)
        {
            packet.SourceElementId = client.Player.Id;
            var otherPlayers = this.weaponBulletSyncMiddleware.GetPlayersToSyncTo(client.Player, packet);
            packet.SendTo(otherPlayers);
        }
    }
}
