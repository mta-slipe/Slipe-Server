using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Scripting;

namespace SlipeServer.Lua;

public static class LuaServiceCollectionExtensions
{
    private static void AddLuaCore(this IServiceCollection services)
    {
        services.AddSingleton<LuaScriptingRuntimeService>();
        services.AddKeyedSingleton<IScriptingRuntimeService, LuaScriptingRuntimeService>("Lua");
        services.AddSingleton<IScriptingRuntimeService>(x => x.GetRequiredService<LuaScriptingRuntimeService>());
        services.AddSingleton<LuaService>();
        services.AddSingleton<IScriptingService, LuaScriptingService>();
        services.AddScripting();
    }

    public static void AddLua(this IServiceCollection services)
    {
        services.AddSingleton<IScriptEventRuntime, ScriptEventRuntime>();
        services.AddSingleton<IScriptInputRuntime, ScriptInputRuntime>();
        services.AddSingleton<ServerResourcesService>();
        services.AddLuaCore();
    }

    public static void AddLua<T>(this IServiceCollection services) where T : class, IScriptEventRuntime
    {
        services.AddSingleton<IScriptEventRuntime, T>();
        services.AddLuaCore();
    }
}
