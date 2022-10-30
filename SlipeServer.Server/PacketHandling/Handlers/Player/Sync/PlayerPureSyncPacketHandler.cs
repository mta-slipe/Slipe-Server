using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using System;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Player.Sync;

public class PlayerPureSyncPacketHandler : IPacketHandler<PlayerPureSyncPacket>
{
    private readonly ILogger logger;
    private readonly ISyncHandlerMiddleware<PlayerPureSyncPacket> pureSyncMiddleware;
    private readonly IElementCollection elementCollection;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_PURESYNC;

    public PlayerPureSyncPacketHandler(
        ILogger logger,
        ISyncHandlerMiddleware<PlayerPureSyncPacket> pureSyncMiddleware,
        IElementCollection elementCollection
    )
    {
        this.logger = logger;
        this.pureSyncMiddleware = pureSyncMiddleware;
        this.elementCollection = elementCollection;
    }

    public void HandlePacket(IClient client, PlayerPureSyncPacket packet)
    {

        if (packet.TimeContext != client.Player.TimeContext && packet.TimeContext > 0 && client.Player.TimeContext > 0)
        {
            this.logger.LogWarning("Received outdated Pure sync packet from {player}", client.Player.Name);
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
            player.PedRotation = packet.Rotation * (180 / MathF.PI);
            player.Velocity = packet.Velocity;
            player.Health = packet.Health;
            player.Armor = packet.Armor;
            player.AimOrigin = packet.AimOrigin;
            player.AimDirection = packet.AimDirection;

            player.ContactElement = this.elementCollection.Get(packet.ContactElementId);
            if (player.ContactElement != null)
            {
                player.Position = player.ContactElement.Position + packet.Position;
            }

            player.CurrentWeaponSlot = (WeaponSlot)packet.WeaponSlot;
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
                var damager = this.elementCollection.Get(packet.DamagerId);
                player.TriggerDamaged(damager, (WeaponType)packet.DamageType, (BodyPart)packet.DamageBodypart);
            }
        });

        player.TriggerSync();
    }
}
