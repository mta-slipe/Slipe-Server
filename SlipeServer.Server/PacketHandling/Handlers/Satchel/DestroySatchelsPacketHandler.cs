using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.Handlers.Satchel;

public class DestroySatchelsPacketHandler(
    ISyncHandlerMiddleware<DestroySatchelsPacket> middleware
    ) : IPacketHandler<DestroySatchelsPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_DESTROY_SATCHELS;

    public void HandlePacket(IClient client, DestroySatchelsPacket packet)
    {
        packet.ElementId = client.Player.Id;

        var otherPlayers = middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);
    }
}
