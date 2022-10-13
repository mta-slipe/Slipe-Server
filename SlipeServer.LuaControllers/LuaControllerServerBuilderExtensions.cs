using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.LuaControllers;

public static class LuaControllerServerBuilderExtensions
{
    public static void AddLuaControllers(this ServerBuilder builder)
    {
        builder.AddLogic<LuaControllerLogic>();
    }
}
