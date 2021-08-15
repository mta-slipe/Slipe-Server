using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware
{
    public class SubscriptionSyncHandlerMiddleware<TData> : ISyncHandlerMiddleware<TData>
    {
        public SubscriptionSyncHandlerMiddleware()
        {
        }

        public IEnumerable<Player> GetPlayersToSyncTo(Player player, TData packet)
        {
            return player.Subscribers;
        }
    }
}
