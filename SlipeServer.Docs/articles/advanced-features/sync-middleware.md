# Sync Middleware
Slipe Server offers an easy way to modify the default sync behaviour.  

MTA uses sync packets to synchronise player movement / driving and shooting. These packets are received by the server, and relayed to other clients.  
Which clients these packets are sent to is decided by packet middleware. 

There are several built-in middlewares:
- [`BasicSyncHandlerMiddleware`](/api/server/SlipeServer.Server.PacketHandling.Handlers.Middleware.BasicSyncHandlerMiddleware-1.html)  
  This is the default middleware, it simply relays to all players except for the source.
- [`RangeSyncHandlerMiddleware`](/api/server/SlipeServer.Server.PacketHandling.Handlers.Middleware.RangeSyncHandlerMiddleware-1.html)  
  This middleware relays to all players within a preset range, and has an optional parameter to specify whether to include the source player.
- [`MaxRangeSyncHandlerMiddleware`](/api/server/SlipeServer.Server.PacketHandling.Handlers.Middleware.MaxRangeSyncHandlerMiddleware-1.html)  
  This middleware relays to all players outside of a preset range, and has an optional parameter to specify whether to include the source player.
- [`SubscriptionSyncHandlerMiddleware`](/api/server/SlipeServer.Server.PacketHandling.Handlers.Middleware.SubscriptionSyncHandlerMiddleware-1.html)  
  This middleware relays to all players that are subscriber to the source players.  
  Players have a `.SubscribeTo(Element element)` method that can be used to influence an element's subscribers. This can be used to completely customise sync logic without having to create additional middlewares.


You can also create your own middleware by creating a class that implements the `ISyncHandlerMiddleware<TData>` interface. Where `TData` is the packet that you want to create the middleware for.

The following example is from the [TDM example gamemode](https://github.com/mta-slipe/slipe-server-examples). 
```cs
public class MatchMiddleware<T> : ISyncHandlerMiddleware<T>
{
    public IEnumerable<Player> GetPlayersToSyncTo(Player player, T data)
    {
        return (player as TdmPlayer)!.Match?.Players.Where(x => x != player) ?? Array.Empty<TdmPlayer>();
    }
}
```
This generic middleware can be applied to any packet, and it will return the other players in the match the player is currently in.

