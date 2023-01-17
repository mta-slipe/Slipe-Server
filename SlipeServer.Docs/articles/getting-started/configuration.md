# Configuration

Slipe Server has several configuration options, these are available in the [`Configuration`](/api/server/SlipeServer.Server.Configuration.html) class.   
You can simple create an instance of this configuration class, and use it in the server like so:
```cs
var configuration = new Configuration()
{
    ServerName = "Slipe Example Server",
};

var server = MtaServer.Create(builder =>
{
    builder.UseConfiguration(configuration);
};
```

It's important that the `UseConfiguration` class is the first call you do on the server builder.  

This configuration is used for many different server config values, such as the port, HTTP port, resource directory, bullet sync config, anticheat, etc.

## Building your server
Besides the configuration itself, there are a lot of other options available on the `ServerBuilder` class, including extensions from other (optional) libraries.  

In other examples we often use `builder.AddDefaults();`. This will set up a sensible list of defaults behaviours, packet handlers, middlewares, resource interpreters, and services. If you want to deviate from this you can either remove the `.AddDefaults()` call, and replace it all manually. (the implementation of `AddDefaults` can be found [here](https://github.com/mta-slipe/Slipe-Server/blob/master/SlipeServer.Server/ServerBuilders/DefaultServerBuilderExtensions.cs)).  

A more sensible approach would be to use the optional parameters the [`AddDefaults`](/api/server/SlipeServer.Server.ServerBuilders.DefaultServerBuilderExtensions.html#SlipeServer_Server_ServerBuilders_DefaultServerBuilderExtensions_AddDefaults_SlipeServer_Server_ServerBuilders_ServerBuilder_SlipeServer_Server_ServerBuilders_ServerBuilderDefaultPacketHandlers_SlipeServer_Server_ServerBuilders_ServerBuilderDefaultBehaviours_SlipeServer_Server_ServerBuilders_ServerBuilderDefaultServices_SlipeServer_Server_ServerBuilders_ServerBuilderDefaultMiddleware_SlipeServer_Server_ServerBuilders_ServerBuilderDefaultResourceInterpreters_) method supports. These allow you to exclude certain defaults from being included.

One example of this might be that you don't want your local development server to announce itself to the master server list, you can achieve this easily using:
```cs
this.server = MtaServer.Create((builder) =>
{
    builder.UseConfiguration(this.configuration);

#if DEBUG
    builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
#else
    builder.AddDefaults();
#endif
});
```

These enums are flag enums, so you can combine multiple exceptions like so:
```cs
this.server = MtaServer.Create((builder) =>
{
    builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour | ServerBuilderDefaultBehaviours.LocalServerAnnouncementBehaviour);
});
```

Furthermore the builder supports some more types to be added, these are:
- Logics  
  Logics are intended to be used to create your actual game logic. Meant to interact with the element classes and (injected) services.
- Behaviours  
  Behaviours are very similar to logics, but thery're meant to replicate behaviour of the server itself (for example master server list announcements, and the relaying of changes to elements to clients. (sendding clients a packet when a vehicle's color changes for example))
- PacketHandlers
  Packet handlers handle packets that are received from clients, these generally update data on elements, trigger events, or just perform logging.  
  You can have multiple handlers for a single packet, so you can add additional logic on top of the default logic.  
- Lua mappings
  Supports defining specific ways to map C# objects to and from Lua values, for server to client communication.
