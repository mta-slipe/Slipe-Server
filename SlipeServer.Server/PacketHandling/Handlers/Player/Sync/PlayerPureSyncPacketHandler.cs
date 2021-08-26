using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware;
using SlipeServer.Server.Repositories;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Player.Sync
{
    public class PlayerPureSyncPacketHandler : IPacketHandler<PlayerPureSyncPacket>
    {
        private readonly ILogger logger;
        private readonly ISyncHandlerMiddleware<PlayerPureSyncPacket> pureSyncMiddleware;
        private readonly IElementRepository elementRepository;

        public PacketId PacketId => PacketId.PACKET_ID_PLAYER_PURESYNC;

        public PlayerPureSyncPacketHandler(
            ILogger logger,
            ISyncHandlerMiddleware<PlayerPureSyncPacket> pureSyncMiddleware,
            IElementRepository elementRepository
        )
        {
            this.logger = logger;
            this.pureSyncMiddleware = pureSyncMiddleware;
            this.elementRepository = elementRepository;
        }

        public void HandlePacket(Client client, PlayerPureSyncPacket packet)
        {

            if (packet.TimeContext != client.Player.TimeContext && packet.TimeContext > 0 && client.Player.TimeContext > 0)
            {
                this.logger.LogWarning($"Received outdated Pure sync packet from {client.Player.Name}");
                return;
            }

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
    }
}
