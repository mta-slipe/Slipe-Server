using Microsoft.Extensions.DependencyInjection;

namespace SlipeServer.Scripting.Luau;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLuauTranspiler(this IServiceCollection services)
    {
        services.AddSingleton<IScriptTransform, LuauToLuaTransform>();
        return services;
    }
}
