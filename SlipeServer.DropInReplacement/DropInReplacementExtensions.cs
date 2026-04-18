using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SlipeServer.DropInReplacement.Console;
using SlipeServer.DropInReplacement.MixedResources;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.DropInReplacement.PacketHandlers;
using SlipeServer.Lua;
using SlipeServer.LuaControllers;
using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Scripting;
using SlipeServer.Server;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Providers;
using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.DropInReplacement;

public static class DropInReplacementExtensions
{
    extension(ServerBuilder builder)
    {
        public ServerBuilder AddDropInReplacementServer()
        {
            builder.AddDefaults(
                exceptResourceInterpreters:
                    ServerBuilderDefaultResourceInterpreters.MetaXml |
                    ServerBuilderDefaultResourceInterpreters.SlipeLua |
                    ServerBuilderDefaultResourceInterpreters.Basic,
                exceptBehaviours:
                    ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour |
                    ServerBuilderDefaultBehaviours.DefaultChatBehaviour |
                    ServerBuilderDefaultBehaviours.PickupBehaviour,
                exceptPacketHandlers:
                    ServerBuilderDefaultPacketHandlers.VehicleInOutPacketHandler |
                    ServerBuilderDefaultPacketHandlers.CommandPacketHandler |
                    ServerBuilderDefaultPacketHandlers.JoinDataPacketHandler,
                includeResourceServer: false,
                includeLogging: false);

            builder.AddPacketHandler<ScriptingVehicleInOutPacketHandler, VehicleInOutPacket>();
            builder.AddPacketHandler<ScriptingCommandPacketHandler, CommandPacket>();
            builder.AddPacketHandler<ScriptingJoinDataPacketHandler, PlayerJoinDataPacket>();
            builder.AddBehaviour<ScriptingPickupBehaviour>();

            builder.AddBehaviour<EventLoggingBehaviour>();

            builder.ConfigureServices((services) =>
            {
                services.AddSingleton<IResourceProvider, DropInReplacementResourceProvider>();;
                services.AddSingleton<IDropInReplacementResourceLuaService, DropInReplacementResourceLuaService>();
                services.AddSingleton<IDropInReplacementResourceService, DropInReplacementResourceService>();
                services.AddSingleton<IResourceService>(sp => sp.GetRequiredService<IDropInReplacementResourceService>());

                services.AddSingleton<ConsoleCommandHandler>();
                services.AddSingleton(sp => new Lazy<ConsoleCommandHandler>(sp.GetRequiredService<ConsoleCommandHandler>));

                services.AddScripting();
                services.AddLua();

                services.AddHttpClient();
            });

            AddLogging(builder);

            builder.AddResourceServer<DropInReplacementResourceServer>();
            builder.AddResourceInterpreter<DropInReplacementResourceInterpreter>();

            builder.AddLuaControllers();

            return builder;
        }

        public void AddLogging()
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<InteractiveConsole>();

                services.AddLogging(x =>
                {
                    if (Environment.UserInteractive)
                        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, InteractiveConsoleLoggerProvider>());
                    else
                        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, NullLoggerProvider>());
                });
                services.TryAddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
            });
        }
    }
}
