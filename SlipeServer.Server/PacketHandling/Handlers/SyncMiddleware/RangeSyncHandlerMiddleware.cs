using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware;

public class RangeSyncHandlerMiddleware<TData> : ISyncHandlerMiddleware<TData>
{
    private readonly IElementCollection elementCollection;
    private readonly float range;
    private readonly bool excludesSource;

    public RangeSyncHandlerMiddleware(IElementCollection elementCollection, float range, bool excludesSource = true)
    {
        this.elementCollection = elementCollection;
        this.range = range;
        this.excludesSource = excludesSource;
    }

    public IEnumerable<Elements.Player> GetPlayersToSyncTo(Elements.Player player, TData packet)
    {
        var elements = this.elementCollection
            .GetWithinRange<Elements.Player>(player.Position, this.range, ElementType.Player);

        if (this.excludesSource)
            return elements.Where(x => x != player);
        return elements;
    }
}
