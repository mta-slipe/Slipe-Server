using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Scripting;

namespace SlipeServer.Lua;

public static class LuaServiceCollectionExtensions
{
    public static void AddLua(this IServiceCollection services)
    {
        services.AddScripting();
        services.AddSingleton<LuaService>();
    }

    public static void AddLua<T>(this IServiceCollection services) where T : class, IScriptEventRuntime
    {
        services.AddScripting<T>();
        services.AddSingleton<LuaService>();
    }
}
