using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.Handlers.BulletSync;

public class WeaponBulletSyncPacketHandler : IPacketHandler<WeaponBulletSyncPacket>
{
    private readonly ISyncHandlerMiddleware<WeaponBulletSyncPacket> middleware;

    public PacketId PacketId => PacketId.PACKET_ID_WEAPON_BULLETSYNC;

    public WeaponBulletSyncPacketHandler(
        ISyncHandlerMiddleware<WeaponBulletSyncPacket> middleware
    )
    {
        this.middleware = middleware;
    }

    public void HandlePacket(Client client, WeaponBulletSyncPacket packet)
    {
        packet.SourceElementId = client.Player.Id;
        var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);
    }
}
