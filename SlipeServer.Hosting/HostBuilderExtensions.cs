using Microsoft.Extensions.Hosting;

namespace SlipeServer.Hosting;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureMtaServers(this IHostBuilder host, Action<IMtaServersConfiguration> configureAction)
    {
        host.ConfigureServices((context, services) =>
        {
            var configuration = new MtaServersConfiguration(context, services);
            configureAction(configuration);
        });

        return host;
    }
}
