using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware;

public class MaxRangeSyncHandlerMiddleware<TData> : ISyncHandlerMiddleware<TData>
{
    private readonly IElementCollection elementCollection;
    private readonly float range;

    public MaxRangeSyncHandlerMiddleware(IElementCollection elementCollection, float range)
    {
        this.elementCollection = elementCollection;
        this.range = range;
    }

    public IEnumerable<Elements.Player> GetPlayersToSyncTo(Elements.Player player, TData packet)
    {
        return this.elementCollection.GetByType<Elements.Player>(ElementType.Player)
            .Except(this.elementCollection
                .GetWithinRange<Elements.Player>(player.Position, this.range, ElementType.Player))
            .Where(x => x != player);
    }
}
