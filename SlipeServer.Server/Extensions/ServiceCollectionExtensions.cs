using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Server.Resources.Features;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Extensions;

public static class ServiceCollectionExtensions
{
    static internal Dictionary<Type, Type> _featureApplier = new();
    public static IServiceCollection AddResourceFeature<TResourceFeature, TResourceFeatureApplier>(this IServiceCollection services)
        where TResourceFeature: IResourceFeature
        where TResourceFeatureApplier: class, IResourceFeatureApplier<TResourceFeature>
    {
        //services.AddSingleton< IResourceFeatureApplier<TResourceFeature>, TResourceFeatureApplier>();
        services.AddSingleton<TResourceFeatureApplier>();
        _featureApplier[typeof(TResourceFeature)] = typeof(TResourceFeatureApplier);
        return services;
    }
}
