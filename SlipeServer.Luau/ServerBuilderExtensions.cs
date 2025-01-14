using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.Scripting.Luau;

public static class ServerBuilderExtensions
{
    public static ServerBuilder AddLuauTranspiler(this ServerBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddLuauTranspiler();
        });

        return builder;
    }
}
