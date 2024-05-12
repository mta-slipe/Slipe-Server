using Microsoft.Extensions.Hosting;

namespace SlipeServer.Hosting;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureMtaServers(this IHostBuilder host, Action<HostBuilderContext, IMtaServersConfiguration> configureAction)
    {
        host.ConfigureServices((context, services) =>
        {
            var configuration = new MtaServersConfiguration(services);
            configureAction(context, configuration);
        });

        return host;
    }

    public static IHostApplicationBuilder ConfigureMtaServers(this IHostApplicationBuilder host, Action<IMtaServersConfiguration> configureAction)
    {
        var configuration = new MtaServersConfiguration(host.Services);
        configureAction(configuration);
        return host;
    }
}
