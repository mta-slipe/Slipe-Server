using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware;

public class RangeSyncHandlerMiddleware<TData>(IElementCollection elementCollection, float range, bool excludesSource = true) : ISyncHandlerMiddleware<TData>
{
    public IEnumerable<Elements.Player> GetPlayersToSyncTo(Elements.Player player, TData packet)
    {
        var elements = elementCollection
            .GetWithinRange<Elements.Player>(player.Position, range, ElementType.Player);

        if (excludesSource)
            return elements.Where(x => x != player);
        return elements;
    }
}
