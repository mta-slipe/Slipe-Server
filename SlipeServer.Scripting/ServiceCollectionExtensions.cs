using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SlipeServer.Scripting.Definitions;

namespace SlipeServer.Scripting;

public static class ServiceCollectionExtensions
{
    public static void AddScripting(this IServiceCollection services)
    {
        services.TryAddSingleton<ScriptTransformationPipeline>();
        services.TryAddSingleton<ISettingsRegistry, SettingsRegistry>();
        services.TryAddSingleton<ScriptTimerService>();
        services.TryAddSingleton<IDevelopmentModeService, DevelopmentModeService>();
        services.TryAddSingleton<IScriptRefService, ScriptRefService>();
        services.TryAddSingleton<ITransferBoxService, TransferBoxService>();
        services.AddSingleton<IScriptEventRuntime, ScriptEventRuntime>();
        services.AddSingleton<IScriptInputRuntime, ScriptInputRuntime>();
        services.TryAddSingleton<IAccountService, SqliteAccountService>();
    }

    public static void AddScripting<T>(this IServiceCollection services) where T : class, IScriptEventRuntime
    {
        services.TryAddSingleton<ScriptTransformationPipeline>();
        services.TryAddSingleton<ISettingsRegistry, SettingsRegistry>();
        services.AddSingleton<IScriptEventRuntime, T>();
    }
}
