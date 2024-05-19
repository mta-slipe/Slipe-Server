using Microsoft.Extensions.DependencyInjection;

namespace SlipeServer.Scripting;

public static class ServiceCollectionExtensions
{
    public static void AddScripting(this IServiceCollection services)
    {
        services.AddSingleton<ServerResourcesService>();
    }
}
