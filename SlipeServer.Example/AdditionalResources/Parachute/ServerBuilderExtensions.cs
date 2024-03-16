using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.Example.AdditionalResources.Parachute;
public static class ServerBuilderExtensions
{
    public static void AddParachuteResource(this ServerBuilder builder)
    {
        builder.AddBuildStep(server =>
        {
            var resource = new ParachuteResource(server);
            server.AddAdditionalResource(resource, resource.AdditionalFiles);
        });
        builder.AddLogic<ParachuteLogic>();
    }
}
