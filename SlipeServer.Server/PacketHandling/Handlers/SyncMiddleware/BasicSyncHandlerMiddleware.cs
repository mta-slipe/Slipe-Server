using SlipeServer.Server.Elements;
using SlipeServer.Server.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware
{
    public class BasicSyncHandlerMiddleware<TData> : ISyncHandlerMiddleware<TData>
    {
        private readonly IElementRepository elementRepository;

        public BasicSyncHandlerMiddleware(IElementRepository elementRepository)
        {
            this.elementRepository = elementRepository;
        }

        public IEnumerable<Elements.Player> GetPlayersToSyncTo(Elements.Player player, TData packet)
        {
            return this.elementRepository
                .GetByType<Elements.Player>(ElementType.Player)
                .Where(x => x != player);
        }
    }
}
