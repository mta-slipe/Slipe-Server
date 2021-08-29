using SlipeServer.Server.Elements;
using SlipeServer.Server.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware
{
    public class MaxRangeSyncHandlerMiddleware<TData> : ISyncHandlerMiddleware<TData>
    {
        private readonly IElementRepository elementRepository;
        private readonly float range;

        public MaxRangeSyncHandlerMiddleware(IElementRepository elementRepository, float range)
        {
            this.elementRepository = elementRepository;
            this.range = range;
        }

        public IEnumerable<Elements.Player> GetPlayersToSyncTo(Elements.Player player, TData packet)
        {
            return this.elementRepository.GetByType<Elements.Player>(ElementType.Player)
                .Except(this.elementRepository
                    .GetWithinRange<Elements.Player>(player.Position, this.range, ElementType.Player))
                .Where(x => x != player);
        }
    }
}
