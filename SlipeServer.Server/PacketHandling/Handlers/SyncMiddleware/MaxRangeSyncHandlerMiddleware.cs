using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware;

public class MaxRangeSyncHandlerMiddleware<TData>(IElementCollection elementCollection, float range) : ISyncHandlerMiddleware<TData>
{
    public IEnumerable<Elements.Player> GetPlayersToSyncTo(Elements.Player player, TData packet)
    {
        return elementCollection.GetByType<Elements.Player>(ElementType.Player)
            .Except(elementCollection
                .GetWithinRange<Elements.Player>(player.Position, range, ElementType.Player))
            .Where(x => x != player);
    }
}
