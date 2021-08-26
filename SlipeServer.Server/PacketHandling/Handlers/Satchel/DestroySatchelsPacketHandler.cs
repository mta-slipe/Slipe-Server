using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware;

namespace SlipeServer.Server.PacketHandling.Handlers.Satchel
{
    public class DestroySatchelsPacketHandler : IPacketHandler<DestroySatchelsPacket>
    {
        private readonly ISyncHandlerMiddleware<DestroySatchelsPacket> middleware;

        public PacketId PacketId => PacketId.PACKET_ID_DESTROY_SATCHELS;

        public DestroySatchelsPacketHandler(
            ISyncHandlerMiddleware<DestroySatchelsPacket> middleware
        )
        {
            this.middleware = middleware;
        }

        public void HandlePacket(Client client, DestroySatchelsPacket packet)
        {
            packet.ElementId = client.Player.Id;

            var otherPlayers = this.middleware.GetPlayersToSyncTo(client.Player, packet);
            packet.SendTo(otherPlayers);
        }
    }
}
