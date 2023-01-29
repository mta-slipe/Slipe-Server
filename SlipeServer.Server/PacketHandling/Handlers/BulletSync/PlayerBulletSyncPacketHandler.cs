using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.Handlers.BulletSync;

public class PlayerBulletSyncPacketHandler : IPacketHandler<PlayerBulletSyncPacket>
{
    private readonly ISyncHandlerMiddleware<PlayerBulletSyncPacket> middleware;

    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_BULLETSYNC;

    public PlayerBulletSyncPacketHandler(
        ISyncHandlerMiddleware<PlayerBulletSyncPacket> middleware
    )
    {
        this.middleware = middleware;
    }

    public void HandlePacket(IClient client, PlayerBulletSyncPacket packet)
    {
        packet.SourceElementId = client.Player.Id;
        var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);
    }
}
