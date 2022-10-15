using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.Resources.Providers;
using System;

namespace SlipeServer.Server.ServerBuilders;

public static class ResourceServerBuilderExtensions
{
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
}
