using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware
{
    public class SubscriptionSyncHandlerMiddleware<TPacket> : ISyncHandlerMiddleware<TPacket> where TPacket : Packet
    {
        public SubscriptionSyncHandlerMiddleware()
        {
        }

        public IEnumerable<Player> GetPlayersToSyncTo(Player player, TPacket packet)
        {
            return player.Subscribers;
        }
    }
}
