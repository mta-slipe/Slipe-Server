using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;

namespace SlipeServer.Server.PacketHandling.Handlers.Player.Sync
{
    public class KeySyncPacketHandler : IPacketHandler<KeySyncPacket>
    {
        private readonly ISyncHandlerMiddleware<KeySyncPacket> middleware;

        public PacketId PacketId => PacketId.PACKET_ID_PLAYER_KEYSYNC;

        public KeySyncPacketHandler(
            ISyncHandlerMiddleware<KeySyncPacket> middleware
        )
        {
            this.middleware = middleware;
        }

        public void HandlePacket(Client client, KeySyncPacket packet)
        {
            packet.PlayerId = client.Player.Id;
            var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);
            packet.SendTo(otherPlayers);
        }
    }
}
