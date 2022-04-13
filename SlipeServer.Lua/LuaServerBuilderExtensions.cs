using SlipeServer.Scripting;
using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.Lua;

public static class LuaServerBuilderExtensions
{
    public static void AddLua(this ServerBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddLua();
        });
    }

    public static void AddLua<T>(this ServerBuilder builder) where T : class, IScriptEventRuntime
    {
        builder.ConfigureServices(services =>
        {
            services.AddLua<T>();
        });
    }
}
