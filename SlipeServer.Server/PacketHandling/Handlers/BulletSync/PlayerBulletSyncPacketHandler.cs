using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using System;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.BulletSync;

public class PlayerBulletSyncPacketHandler : IPacketHandler<PlayerBulletSyncPacket>
{
    private readonly ISyncHandlerMiddleware<PlayerBulletSyncPacket> middleware;
    private readonly ILogger logger;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_BULLETSYNC;

    public PlayerBulletSyncPacketHandler(
        ISyncHandlerMiddleware<PlayerBulletSyncPacket> middleware,
        ILogger logger
    )
    {
        this.middleware = middleware;
        this.logger = logger;
    }

    public void HandlePacket(IClient client, PlayerBulletSyncPacket packet)
    {
        packet.SourceElementId = client.Player.Id;
        var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);

        if (!client.Player.Weapons.Any(x => x.Type == (WeaponId)packet.WeaponType))
        {
            this.logger.LogWarning("Potential cheating, player {player} attempt to fire a {weapon} that they do not posess", client.Player.Name, (WeaponId)packet.WeaponType);
            return;
        }
        packet.SendTo(otherPlayers);
    }
}
