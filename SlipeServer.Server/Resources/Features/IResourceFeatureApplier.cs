using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Resources.Features;

internal static class ResourceFeatures
{
    internal static Dictionary<Type, Type> resourceFeatureAppliers = new();
}

public interface IResourceFeatureApplier<TFeature> where TFeature : IResourceFeature
{
    void Apply(TFeature feature, Resource resource, Dictionary<string, byte[]> files);
}
