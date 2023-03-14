using System.Collections.Generic;

namespace SlipeServer.Server.Resources.Features;

public interface IResourceFeatureApplier<TFeature> where TFeature : IResourceFeature
{
    void Apply(TFeature feature, Resource resource, Dictionary<string, byte[]> files);
}
