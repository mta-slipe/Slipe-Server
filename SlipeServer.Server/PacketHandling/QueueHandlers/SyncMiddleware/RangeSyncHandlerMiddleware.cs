using SlipeServer.Packets;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware
{
    public class RangeSyncHandlerMiddleware<TData> : ISyncHandlerMiddleware<TData>
    {
        private readonly IElementRepository elementRepository;
        private readonly float range;

        public RangeSyncHandlerMiddleware(IElementRepository elementRepository, float range)
        {
            this.elementRepository = elementRepository;
            this.range = range;
        }

        public IEnumerable<Player> GetPlayersToSyncTo(Player player, TData packet)
        {
            return this.elementRepository
                .GetWithinRange<Player>(player.Position, this.range, ElementType.Player)
                .Where(x => x != player);
        }
    }
}
