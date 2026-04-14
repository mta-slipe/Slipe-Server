using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.Handlers.BulletSync;

public class WeaponBulletSyncPacketHandler(
    ISyncHandlerMiddleware<WeaponBulletSyncPacket> middleware,
    IElementCollection elementCollection
    ) : IPacketHandler<WeaponBulletSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_WEAPON_BULLETSYNC;

    public void HandlePacket(IClient client, WeaponBulletSyncPacket packet)
    {
        var element = elementCollection.Get(packet.WeaponElementId);

        if (element is not WeaponObject weapon)
            return;

        if (weapon.Owner != client.Player)
            return;

        if (weapon.Ammo <= 0)
            return;

        if (weapon.ClipAmmo <= 0)
            return;

        weapon.TriggerFired();

        packet.SourceElementId = client.Player.Id;
        var otherPlayers = middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);
    }
}

