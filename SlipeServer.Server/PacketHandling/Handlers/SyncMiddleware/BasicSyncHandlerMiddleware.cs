using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware;

public class BasicSyncHandlerMiddleware<TData> : ISyncHandlerMiddleware<TData>
{
    private readonly IElementCollection elementRepository;

    public BasicSyncHandlerMiddleware(IElementCollection elementRepository)
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
