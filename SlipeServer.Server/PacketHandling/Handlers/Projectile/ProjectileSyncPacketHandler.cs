using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.Handlers.Projectile;

public class ProjectileSyncPacketHandler : IPacketHandler<ProjectileSyncPacket>
{
    private readonly ISyncHandlerMiddleware<ProjectileSyncPacket> middleware;

    public PacketId PacketId => PacketId.PACKET_ID_PROJECTILE;

    public ProjectileSyncPacketHandler(
        ISyncHandlerMiddleware<ProjectileSyncPacket> middleware
    )
    {
        this.middleware = middleware;
    }

    public void HandlePacket(IClient client, ProjectileSyncPacket packet)
    {
        var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SourceElement = client.Player.Id;
        packet.SendTo(otherPlayers);
    }
}
