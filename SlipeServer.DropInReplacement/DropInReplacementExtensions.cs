using Microsoft.Extensions.DependencyInjection;
using SlipeServer.DropInReplacement.MixedResources;
using SlipeServer.DropInReplacement.MixedResources.Behaviour;
using SlipeServer.Lua;
using SlipeServer.LuaControllers;
using SlipeServer.Scripting;
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
                exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour,
                includeResourceServer: false);

            builder.ConfigureServices((services) =>
            {
                services.AddSingleton<IResourceProvider, DropInReplacementResourceProvider>();
                services.AddSingleton<IDropInReplacementResourceService, DropInReplacementResourceService>();
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
