using SlipeServer.Console.AdditionalResources.Parachute;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.Console.AdditionalResources.ResourceWithFeatures;

public static class ServerBuilderExtensions
{
    public static void AddResourceWithFeatures(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new ResourceWithFeaturesResource(server);
            server.AddAdditionalResourceWithFeatures(resource, resource.AdditionalFiles);
        });
        builder.AddLogic<ResourceWithFeaturesLogic>();
    }
}
