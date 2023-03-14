using SlipeServer.Server.Resources;
using SlipeServer.Server.Resources.Features;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SlipeServer.Server.Extensions;

public static class MtaServerExtensions
{
    /// <summary>
    /// Adds an additional resource to the server, which then gets registered with all resource servers.
    /// Applies all features requested by resource.
    /// </summary>
    public static void AddAdditionalResourceWithFeatures(this MtaServer server, Resource resource, Dictionary<string, byte[]> files)
    {
        var implementedFeatures = resource.GetType()
            .GetInterfaces()
            .Where(x => x.IsAssignableTo(typeof(IResourceFeature)) && x != typeof(IResourceFeature))
            .ToList();

        foreach (var featureType in implementedFeatures)
        {
            var featureApplierType = ServiceCollectionExtensions._featureApplier[featureType];

            var featureApplierInstance = server.GetRequiredService(featureApplierType);
            featureApplierInstance.GetType().GetMethod("Apply").Invoke(featureApplierInstance, new object[] { resource, resource, files });
        }

        server.AddAdditionalResource(resource, files);
    }
}
