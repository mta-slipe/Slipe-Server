using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.Handlers.BulletSync;

public class WeaponBulletSyncPacketHandler(
    ISyncHandlerMiddleware<WeaponBulletSyncPacket> middleware
    ) : IPacketHandler<WeaponBulletSyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_WEAPON_BULLETSYNC;

    public void HandlePacket(IClient client, WeaponBulletSyncPacket packet)
    {
        packet.SourceElementId = client.Player.Id;
        var otherPlayers = middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);
    }
}
