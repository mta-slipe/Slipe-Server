using SlipeServer.LuaControllers;
using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.Example;

public static class ServerBuilderExtensions
{
    public static ServerBuilder AddExampleLogic(this ServerBuilder builder)
    {
        builder.AddLogic<ServerExampleLogic>();
        builder.AddLuaControllers();
        builder.AddLogic<ResourcesLogic>();

        return builder;
    }
}
