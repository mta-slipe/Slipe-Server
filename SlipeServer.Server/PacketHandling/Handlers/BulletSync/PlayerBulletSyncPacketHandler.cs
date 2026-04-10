using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ElementCollections;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.BulletSync;

public class PlayerBulletSyncPacketHandler(
    ISyncHandlerMiddleware<PlayerBulletSyncPacket> middleware,
    ILogger logger,
    IElementCollection elementCollection
    ) : IPacketHandler<PlayerBulletSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_BULLETSYNC;

    public void HandlePacket(IClient client, PlayerBulletSyncPacket packet)
    {
        packet.SourceElementId = client.Player.Id;
        var otherPlayers = middleware.GetPlayersToSyncTo(client.Player, packet);

        if (!client.Player.Weapons.Any(x => x.Type == (WeaponId)packet.WeaponType))
        {
            logger.LogWarning("Potential cheating, player {player} attempt to fire a {weapon} that they do not posess", client.Player.Name, (WeaponId)packet.WeaponType);
            return;
        }

        var hitElement = packet.DamagedElementId.HasValue ? elementCollection.Get(packet.DamagedElementId.Value) : null;
        client.Player.TriggerWeaponFired((WeaponId)packet.WeaponType, packet.Start, packet.End, hitElement);

        packet.SendTo(otherPlayers);
    }
}
