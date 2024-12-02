using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.Example;

public static class ServerBuilderExtensions
{
    public static ServerBuilder AddExampleLogic(this ServerBuilder builder)
    {
        builder.AddLogic<ServerExampleLogic>();
        builder.AddLogic<ResourcesLogic>();

        return builder;
    }
}
