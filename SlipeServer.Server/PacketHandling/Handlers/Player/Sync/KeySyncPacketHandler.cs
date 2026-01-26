using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Clients;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.Handlers.Player.Sync;

public class KeySyncPacketHandler(
    ISyncHandlerMiddleware<KeySyncPacket> middleware
    ) : IPacketHandler<KeySyncPacket>
{
    public PacketId PacketId => PacketId.PACKET_ID_PLAYER_KEYSYNC;

    public void HandlePacket(IClient client, KeySyncPacket packet)
    {
        packet.PlayerId = client.Player.Id;
        var otherPlayers = middleware.GetPlayersToSyncTo(client.Player, packet);
        packet.SendTo(otherPlayers);
    }
}
