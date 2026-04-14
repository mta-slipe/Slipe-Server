using Microsoft.Extensions.DependencyInjection;
using SlipeServer.DropInReplacement.MixedResources;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.DropInReplacement.PacketHandlers;
using SlipeServer.Lua;
using SlipeServer.LuaControllers;
using SlipeServer.Packets.Definitions.Commands;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Definitions.Vehicles;
using SlipeServer.Scripting;
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
                includeResourceServer: false);

            builder.AddPacketHandler<ScriptingVehicleInOutPacketHandler, VehicleInOutPacket>();
            builder.AddPacketHandler<ScriptingCommandPacketHandler, CommandPacket>();
            builder.AddPacketHandler<ScriptingJoinDataPacketHandler, PlayerJoinDataPacket>();
            builder.AddBehaviour<ScriptingPickupBehaviour>();

            builder.ConfigureServices((services) =>
            {
                services.AddSingleton<IResourceProvider, DropInReplacementResourceProvider>();
                services.AddSingleton<DropInReplacementResourceService>();
                services.AddSingleton<IDropInReplacementResourceService>(sp => sp.GetRequiredService<DropInReplacementResourceService>());
                services.AddSingleton<IResourceService>(sp => sp.GetRequiredService<DropInReplacementResourceService>());
                services.AddSingleton<IDropInReplacementResourceLuaService, DropInReplacementResourceLuaService>();

                services.AddScripting();
                services.AddLua();

                services.AddHttpClient();
            });

            builder.AddResourceServer<DropInReplacementResourceServer>();
            builder.AddResourceInterpreter<DropInReplacementResourceInterpreter>();

            builder.AddLuaControllers();

            return builder;
        }
    }
}
