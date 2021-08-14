using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware
{
    public class BasicSyncHandlerMiddleware<TData> : ISyncHandlerMiddleware<TData>
    {
        private readonly IElementRepository elementRepository;

        public BasicSyncHandlerMiddleware(IElementRepository elementRepository)
        {
            this.elementRepository = elementRepository;
        }

        public IEnumerable<Player> GetPlayersToSyncTo(Player player, TData packet)
        {
            return this.elementRepository
                .GetByType<Player>(ElementType.Player)
                .Where(x => x != player);
        }
    }
}
