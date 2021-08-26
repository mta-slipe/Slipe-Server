using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware
{
    public class SubscriptionSyncHandlerMiddleware<TData> : ISyncHandlerMiddleware<TData>
    {
        public SubscriptionSyncHandlerMiddleware()
        {
        }

        public IEnumerable<Elements.Player> GetPlayersToSyncTo(Elements.Player player, TData packet)
        {
            return player.Subscribers;
        }
    }
}
