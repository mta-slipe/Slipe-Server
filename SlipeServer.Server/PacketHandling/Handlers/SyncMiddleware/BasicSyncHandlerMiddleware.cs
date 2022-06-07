using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware;

public class BasicSyncHandlerMiddleware<TData> : ISyncHandlerMiddleware<TData>
{
    private readonly IElementCollection elementCollection;

    public BasicSyncHandlerMiddleware(IElementCollection elementCollection)
    {
        this.elementCollection = elementCollection;
    }

    public IEnumerable<Elements.Player> GetPlayersToSyncTo(Elements.Player player, TData packet)
    {
        return this.elementCollection
            .GetByType<Elements.Player>(ElementType.Player)
            .Where(x => x != player);
    }
}
