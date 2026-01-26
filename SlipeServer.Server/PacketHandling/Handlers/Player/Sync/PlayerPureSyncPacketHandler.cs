using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using System;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Player.Sync;

public class PlayerPureSyncPacketHandler(
    ILogger logger,
    ISyncHandlerMiddleware<PlayerPureSyncPacket> pureSyncMiddleware,
    IElementCollection elementCollection,
    Configuration configuration
    ) : IPacketHandler<PlayerPureSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_PURESYNC;

    public void HandlePacket(IClient client, PlayerPureSyncPacket packet)
    {
        if (!IsTimeSyncContextValid(client, packet.TimeContext))
            return;

        var player = client.Player;
        if (player.Vehicle != null && player.VehicleAction != VehicleAction.Exiting)
            return;

        if(player.ShouldSendReturnSyncPacket())
            client.SendPacket(new ReturnSyncPacket(packet.Position));

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

            player.ContactElement = elementCollection.Get(packet.ContactElementId);
            if (player.ContactElement != null)
                player.Position = player.ContactElement.Position + packet.Position;
            else
            {
                var element = player.AssociatedElements
                    .Get(packet.ContactElementId);

                player.Position = (element?.Position + packet.Position) ?? packet.Position;
            }

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
                var damager = elementCollection.Get(packet.DamagerId);
                var loss = (previousHealth - packet.Health) + (previousArmor - packet.Armor);

                player.TriggerDamaged(damager, (DamageType)packet.DamageType, (BodyPart)packet.DamageBodypart, loss);
            }
        });

        player.TriggerSync();

        var otherPlayers = pureSyncMiddleware.GetPlayersToSyncTo(client.Player, packet);
        if (otherPlayers.Any())
            packet.SendTo(otherPlayers);
    }

    private bool IsTimeSyncContextValid(IClient client, byte context)
    {
        if (context != client.Player.TimeContext && context > 0 && client.Player.TimeContext > 0)
        {
            lock (client.Player.TimeContextFailureCountLock)
            {
                client.Player.TimeContextFailureCount++;

                if (client.Player.TimeContextFailureCount > 100)
                {
                    client.Player.TimeContextFailureCount = 0;
                    logger.LogError("Received mismatching Pure sync packet from {player} many times, local: {local}, remote: {remote}", client.Player.Name, client.Player.TimeContext, context);

                    if (configuration.Debug.AutoResolveTimeSyncContextMismatches)
                        client.Player.OverrideTimeContext(context);
                }
            }
            logger.LogTrace("Received mismatching Pure sync packet from {player}, local: {local}, remote: {remote}", client.Player.Name, client.Player.TimeContext, context);
            return false;
        }

        lock (client.Player.TimeContextFailureCountLock)
        {
            client.Player.TimeContextFailureCount = 0;
        }

        return true;
    }
}
