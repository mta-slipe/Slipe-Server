using Microsoft.Extensions.DependencyInjection;
using SlipeServer.Physics.Services;
using SlipeServer.Server.ServerBuilders;

namespace SlipeServer.Physics.Extensions;

public static class PhysicsServerBuilderExtensions
{
    public static void AddPhysics(this ServerBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<PhysicsService>();
        });
    }
}
