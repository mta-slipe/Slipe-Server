using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.Resources.Features;

namespace SlipeServer.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResourceFeature<TResourceFeature, TResourceFeatureApplier>(this IServiceCollection services)
        where TResourceFeature: IResourceFeature
        where TResourceFeatureApplier: class, IResourceFeatureApplier<TResourceFeature>
    {
        services.AddSingleton<TResourceFeatureApplier>();
        ResourceFeatures.resourceFeatureAppliers[typeof(TResourceFeature)] = typeof(TResourceFeatureApplier);
        return services;
    }
}
