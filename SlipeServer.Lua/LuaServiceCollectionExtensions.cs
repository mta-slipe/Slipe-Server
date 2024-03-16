using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Scripting;

namespace SlipeServer.Lua;

public static class LuaServiceCollectionExtensions
{
    public static void AddLua(this IServiceCollection services)
    {
        services.AddSingleton<IScriptEventRuntime, ScriptEventRuntime>();
        services.AddSingleton<IScriptInputRuntime, ScriptInputRuntime>();
        services.AddSingleton<LuaService>();
    }

    public static void AddLua<T>(this IServiceCollection services) where T : class, IScriptEventRuntime
    {
        services.AddSingleton<IScriptEventRuntime, T>();
        services.AddSingleton<LuaService>();
    }
}
