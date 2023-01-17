# Injectable Services

As stated before, there are several services that can be injected into your Logics (or other classes). These offer various ways of interacting with the server. At the end of the article is a list of these services, the list contains links to the individual docs for every one of these classes.

## Injecting
You can inject these classes into any class that is created by the dependency injection container. This includes logics created by `.AddLogic()`, and any of its dependencies.  

## Setting up additional dependencies
You can also register additional classes and/or services with the server's dependency injection container, you do this in your Program.cs like so:
```cs
var server = MtaServer.CreateWithDiSupport<FreeroamPlayer>(builder =>
{
    builder.AddDefaults();

    builder.ConfigureServices(services =>
    {
        // register your services here 
        services.AddSingleton<ILogger, ConsoleLogger>();
    });
}
```

## Next?
You do not need to know all of these in order to get started, but this can be good reference to see what's available to you.  
Next up we recommend reading [Custom player types](/articles/getting-started/custom-player-types.html)

## List

### Services
- [ChatBox](/api/server/SlipeServer.Server.Services.ChatBox.html)  
  Allows you to send messages to the chatbox, and modify chatbox visibility.
- [ClientConsole](/api/server/SlipeServer.Server.Services.ClientConsole.html)  
  Allows you to send messages to a player's F8 console.
- [CommandService](/api/server/SlipeServer.Server.Services.CommandService.html)  
  Allows you to create and remove command handlers.
- [DebugLog](/api/server/SlipeServer.Server.Services.DebugLog.html)  
  Allows you to send messages to the debug log, and modify debug log visibility.
- [ExplosionService](/api/server/SlipeServer.Server.Services.ExplosionService.html)  
  Allows you to create explosions.
- [FireService](/api/server/SlipeServer.Server.Services.FireService.html)  
  Allows you to create fires.
- [GameWorld](/api/server/SlipeServer.Server.Services.GameWorld.html)  
  Allows you to modify properties related to the game world, including as: 
  - weather
  - game speed
  - fps limit
  - gravity
  - world object removals
  - and more
- [LatentPacketService](/api/server/SlipeServer.Server.Services.LatentPacketService.html)  
  Used internally to handle latent packets.
- [LuaEventService](/api/server/SlipeServer.Server.Services.LuaEventService.html)  
  Allows you to trigger Lua events for one or more players, for server to client Lua communication.  
  Also allows you to create handlers for Lua events being triggered from clients.
- [TextItemService](/api/server/SlipeServer.Server.Services.TextItemService.html)  
  Allows you to create 2D Text items for players.
- [WeaponConfigurationService](/api/server/SlipeServer.Server.Services.WeaponConfigurationService.html)  
  Allows you to get and modify weapon configurations.

### Lua value mapping
- [LuaValueMapper](/api/server/SlipeServer.Server.Mappers.LuaValueMapper.html)  
  Allows you to map any C# type to a LuaValue, and allows you to define custom mapping logic for specific types.
- [FromLuaValueMapper](/api/server/SlipeServer.Server.Mappers.FromLuaValueMapper.html)  
  Allows you to map LuaValues to any C# type, and allows you to define custom mapping logic for specific types.

### Misc
- [RootElement](/api/server/SlipeServer.Server.Elements.RootElement.html)  
  The Element that represents the root element, the element at hte very top of the element tree.
- [MtaServer](/api/server/SlipeServer.Server.Elements.RootElement.html)  
  The MTA server itself that's running, useful for global server-wide events like `PlayerJoined` and `ElementCreated`, or other server specific methods.
- [Configuration](/api/server/SlipeServer.Server.Configuration.html)  
  The servers configuration, containing fiels like the host, port, anticheat settings, etc.
- [HttpClient](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)  
  C# Built in HTTP client for making HTTP requests.
- [IElementCollection](/api/server/SlipeServer.Server.ElementCollections.IElementCollection.html)  
  Collection that keeps track of all elements associated with the server. You can use this to get elements by type, by id, in range of a location, and more.
- [ILogger](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger)  
  Interface for logging messages to the server console.
