using Microsoft.Extensions.DependencyInjection;
using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Player;
using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Definitions.Transgression;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.PacketHandling.Handlers.AntiCheat;
using SlipeServer.Server.PacketHandling.Handlers.BulletSync;
using SlipeServer.Server.PacketHandling.Handlers.Command;
using SlipeServer.Server.PacketHandling.Handlers.Connection;
using SlipeServer.Server.PacketHandling.Handlers.Explosions;
using SlipeServer.Server.PacketHandling.Handlers.Lua;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.PacketHandling.Handlers.Player;
using SlipeServer.Server.PacketHandling.Handlers.Player.Sync;
using SlipeServer.Server.PacketHandling.Handlers.Projectile;
using SlipeServer.Server.PacketHandling.Handlers.Rpc;
using SlipeServer.Server.PacketHandling.Handlers.Satchel;
using SlipeServer.Server.PacketHandling.Handlers.Vehicle;
using SlipeServer.Server.PacketHandling.Handlers.Vehicle.Sync;
using SlipeServer.Server.PacketHandling.Handlers.Voice;
using SlipeServer.Server.ElementCollections;
using System.IO;
using SlipeServer.Packets.Definitions.Ped;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using SlipeServer.Server.PacketHandling.Handlers.CustomData;
using SlipeServer.Packets.Definitions.CustomElementData;
using SlipeServer.Packets.Definitions.Resources;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.Resources.Interpreters;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SlipeServer.Server.Loggers;

namespace SlipeServer.Server.ServerBuilders;

public static class DefaultServerBuilderExtensions
{
    public static void AddDefaultPacketHandler(
        this ServerBuilder builder,
        ServerBuilderDefaultPacketHandlers except = ServerBuilderDefaultPacketHandlers.None)
    {
        builder.AddBuildStep(server => server.AddDefaultPacketHandlers(except));
    }

    public static void AddDefaultBehaviours(
        this ServerBuilder builder,
        ServerBuilderDefaultBehaviours except = ServerBuilderDefaultBehaviours.None)
    {
        builder.AddBuildStep(server => server.AddDefaultBehaviours(except));
    }

    public static void AddDefaultServices(
        this ServerBuilder builder,
        ServerBuilderDefaultServices exceptServices = ServerBuilderDefaultServices.None,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None)
    {
        builder.ConfigureServices(services =>
        {
            services.AddDefaultMiddlewares(exceptMiddleware);
        });
    }

    public static void AddDefaultResourceInterpreters(
        this ServerBuilder builder,
        ServerBuilderDefaultResourceInterpreters except = ServerBuilderDefaultResourceInterpreters.None)
    {
        if (!except.HasFlag(ServerBuilderDefaultResourceInterpreters.Basic))
            builder.AddResourceInterpreter<BasicResourceInterpreter>();

        if (!except.HasFlag(ServerBuilderDefaultResourceInterpreters.MetaXml))
            builder.AddResourceInterpreter<MetaXmlResourceInterpreter>();

        if (!except.HasFlag(ServerBuilderDefaultResourceInterpreters.SlipeLua))
            builder.AddResourceInterpreter<SlipeLuaResourceInterpreter>();
    }

    public static void AddDefaultLogging(this ServerBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddLogging(x =>
            {
                if (Environment.UserInteractive)
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ConsoleLoggerProvider>());
                else
                    services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, NullLoggerProvider>());
            });
            services.TryAddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
        });
    }

    /// <summary>
    /// Registers all default packet handlers, behaviours, services, lua mappings, resource servers, resource interpreters, and networking interfaces
    /// More information can be found on https://server.mta-slipe.com/articles/getting-started/configuration.html#building-your-server
    /// </summary>
    /// <param name="exceptPacketHandlers">Packet handlers to exclude</param>
    /// <param name="exceptBehaviours">Behaviours to exclude</param>
    /// <param name="exceptServices">Services to exclude</param>
    /// <param name="exceptMiddleware">Middleware to exclude</param>
    /// <param name="exceptResourceInterpreters">Resource interpreters to exclude</param>
    public static void AddDefaults(
        this ServerBuilder builder,
        ServerBuilderDefaultPacketHandlers exceptPacketHandlers = ServerBuilderDefaultPacketHandlers.None,
        ServerBuilderDefaultBehaviours exceptBehaviours = ServerBuilderDefaultBehaviours.None,
        ServerBuilderDefaultServices exceptServices = ServerBuilderDefaultServices.None,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None,
        ServerBuilderDefaultResourceInterpreters exceptResourceInterpreters = ServerBuilderDefaultResourceInterpreters.None,
        bool includeLogging = true)
    {
        builder.AddDefaultPacketHandler(exceptPacketHandlers);
        builder.AddDefaultBehaviours(exceptBehaviours);
        builder.AddDefaultServices(exceptServices, exceptMiddleware);
        builder.AddDefaultLuaMappings();

        builder.AddResourceServer<BasicHttpServer>();
        builder.AddDefaultResourceInterpreters(exceptResourceInterpreters);

        builder.AddDefaultNetWrapper();

        if (includeLogging)
            AddDefaultLogging(builder);
    }

    /// <summary>
    /// Registers all default packet handlers, behaviours, services, lua mappings, resource servers, resource interpreters, and networking interfaces
    /// More information can be found on https://server.mta-slipe.com/articles/getting-started/configuration.html#building-your-server
    /// </summary>
    /// <param name="exceptPacketHandlers">Packet handlers to exclude</param>
    /// <param name="exceptBehaviours">Behaviours to exclude</param>
    /// <param name="exceptServices">Services to exclude</param>
    /// <param name="exceptMiddleware">Middleware to exclude</param>
    /// <param name="exceptResourceInterpreters">Resource interpreters to exclude</param>
    public static void AddHostedDefaults(
        this ServerBuilder builder,
        ServerBuilderDefaultPacketHandlers exceptPacketHandlers = ServerBuilderDefaultPacketHandlers.None,
        ServerBuilderDefaultBehaviours exceptBehaviours = ServerBuilderDefaultBehaviours.None,
        ServerBuilderDefaultMiddleware exceptMiddleware = ServerBuilderDefaultMiddleware.None,
        ServerBuilderDefaultResourceInterpreters exceptResourceInterpreters = ServerBuilderDefaultResourceInterpreters.None,
        bool includeLogging = true)
    {
        builder.AddDefaultPacketHandler(exceptPacketHandlers);
        builder.AddDefaultBehaviours(exceptBehaviours);
        builder.AddDefaultLuaMappings();

        builder.AddResourceServer<BasicHttpServer>();
        builder.AddDefaultResourceInterpreters(exceptResourceInterpreters);

        builder.AddDefaultNetWrapper();

        if (includeLogging)
            AddDefaultLogging(builder);
    }

    public static void AddDefaultNetWrapper(this ServerBuilder builder)
    {
        builder.AddNetWrapper(
            Directory.GetCurrentDirectory(),
            "net",
            builder.Configuration.Host,
            builder.Configuration.Port,
            builder.Configuration.AntiCheat);

        if (builder.Configuration.DebugPort.HasValue && !Environment.Is64BitProcess)
            builder.AddNetWrapper(dllPath: "net_d", port: builder.Configuration.DebugPort.Value);
    }
}
