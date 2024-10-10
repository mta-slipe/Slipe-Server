using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources.Providers;

namespace SlipeServer.Server.ServerBuilders;

public static class ResourceServerBuilderExtensions
{
    /// <summary>
    /// Adds a resource interpreter
    /// More information can be found on https://server.mta-slipe.com/articles/getting-started/lua-resources.html#interpreters
    /// </summary>
    /// <typeparam name="TResourceInterpreter"></typeparam>
    /// <param name="builder"></param>
    public static void AddResourceInterpreter<TResourceInterpreter>(this ServerBuilder builder)
        where TResourceInterpreter : class, IResourceInterpreter
    {

        builder.AddBuildStep(x =>
        {
            var provider = x.GetRequiredService<IResourceProvider>();
            provider.AddResourceInterpreter(x.Instantiate<TResourceInterpreter>());
            provider.Refresh();
        }, ServerBuildStepPriority.High);
    }

    /// <summary>
    /// Adds a resource interpreter
    /// More information can be found on https://server.mta-slipe.com/articles/getting-started/lua-resources.html#interpreters
    /// </summary>
    /// <typeparam name="TResourceInterpreter"></typeparam>
    /// <param name="builder"></param>
    public static void AddResourceInterpreter<TResourceInterpreter>(this IServiceCollection services)
        where TResourceInterpreter : class, IResourceInterpreter
    {
        services.AddSingleton<IResourceInterpreter, TResourceInterpreter>();
    }
}
