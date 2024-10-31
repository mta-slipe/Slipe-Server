using Microsoft.Extensions.DependencyInjection;
using SlipeServer.LuaControllers.Commands;
using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.LuaControllers;

public static class LuaControllerServerBuilderExtensions
{
    public static ServerBuilder AddLuaControllers(this ServerBuilder builder)
    {
        builder.AddLogic<LuaControllerLogic>();
        builder.AddLogic<CommandControllerLogic>();

        builder.ConfigureServices(services =>
        {
            services.AddLuaControllers();
        });

        return builder;
    }
}


public static class LuaControllerServiceCollectionExtensions
{
    public static IServiceCollection AddLuaControllers(this IServiceCollection services)
    {
        services.AddSingleton<LuaControllerArgumentsMapper>();
        return services;
    }
}
