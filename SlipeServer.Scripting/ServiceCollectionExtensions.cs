using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SlipeServer.Scripting;

public static class ServiceCollectionExtensions
{
    public static void AddScripting(this IServiceCollection services)
    {
        services.TryAddSingleton<ScriptTransformationPipeline>();
        services.AddSingleton<IScriptEventRuntime, ScriptEventRuntime>();
        services.AddSingleton<IScriptInputRuntime, ScriptInputRuntime>();
    }

    public static void AddScripting<T>(this IServiceCollection services) where T : class, IScriptEventRuntime
    {
        services.TryAddSingleton<ScriptTransformationPipeline>();
        services.AddSingleton<IScriptEventRuntime, T>();
    }
}
