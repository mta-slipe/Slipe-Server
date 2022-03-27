using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling.Handlers.Middleware;

public interface ISyncHandlerMiddleware<TData>
{
    IEnumerable<Elements.Player> GetPlayersToSyncTo(Elements.Player player, TData data);
}
