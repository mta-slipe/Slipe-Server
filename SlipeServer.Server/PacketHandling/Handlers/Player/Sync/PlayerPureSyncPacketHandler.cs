using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
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
            this.logger.LogTrace("Received mismatching Pure sync packet from {player}, local: {local}, remote: {remote}", client.Player.Name, client.Player.TimeContext, packet.TimeContext);
            return;
        }

        var player = client.Player;
        player.IncrementReturnSyncPacket();

        if (player.Vehicle != null && player.VehicleAction != VehicleAction.Exiting)
            return;

        if(player.ShouldSendReturnSyncPacket())
            client.SendPacket(SyncPacketFactory.CreateReturnSyncPacket(client.Player));

        packet.PlayerId = client.Player.Id;
        packet.Latency = (ushort)client.Ping;

        player.RunAsSync(() =>
        {
            var previousHealth = player.Health;
            var previousArmor = player.Armor;

            player.PedRotation = packet.Rotation * (180 / MathF.PI);
            player.Velocity = packet.Velocity;
            player.Health = packet.Health;
            player.Armor = packet.Armor;
            player.AimOrigin = packet.AimOrigin;
            player.AimDirection = packet.AimDirection;

            player.ContactElement = this.elementCollection.Get(packet.ContactElementId);
            if (player.ContactElement != null)
                player.Position = player.ContactElement.Position + packet.Position;
            else
                player.Position = packet.Position;

            var weaponChanged = player.CurrentWeaponSlot != (WeaponSlot)packet.WeaponSlot;
            player.CurrentWeaponSlot = (WeaponSlot)packet.WeaponSlot;
            if (player.CurrentWeapon != null && player.CurrentWeapon.Type == (WeaponId)packet.WeaponType)
            {
                var ammoChanged = player.CurrentWeapon.Ammo != packet.TotalAmmo || player.CurrentWeapon.AmmoInClip != packet.AmmoInClip;
                player.CurrentWeapon.Ammo = packet.TotalAmmo;
                player.CurrentWeapon.AmmoInClip = packet.AmmoInClip;

                if (weaponChanged || ammoChanged)
                    player.TriggerWeaponAmmoUpdate((WeaponId)packet.WeaponType, packet.TotalAmmo, packet.AmmoInClip);
            } else if (packet.WeaponSlot != 0)
            {
                packet.WeaponSlot = 0;
                packet.WeaponType = 0;
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

            player.LastMovedUtc = DateTime.UtcNow;

            if (packet.IsDamageChanged)
            {
                var damager = this.elementCollection.Get(packet.DamagerId);
                var loss = (previousHealth - packet.Health) + (previousArmor - packet.Armor);

                player.TriggerDamaged(damager, (DamageType)packet.DamageType, (BodyPart)packet.DamageBodypart, loss);
            }
        });

        player.TriggerSync();

        var otherPlayers = this.pureSyncMiddleware.GetPlayersToSyncTo(client.Player, packet);
        if (otherPlayers.Any())
            packet.SendTo(otherPlayers);
    }
}
