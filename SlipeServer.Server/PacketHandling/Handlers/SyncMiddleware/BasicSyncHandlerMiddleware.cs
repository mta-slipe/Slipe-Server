using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware;

public class BasicSyncHandlerMiddleware<TData>(IElementCollection elementCollection) : ISyncHandlerMiddleware<TData>
{
    public IEnumerable<Elements.Player> GetPlayersToSyncTo(Elements.Player player, TData packet)
    {
        return elementCollection
            .GetByType<Elements.Player>(ElementType.Player)
            .Where(x => x != player);
    }
}
