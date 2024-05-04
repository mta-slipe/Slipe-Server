using Microsoft.Extensions.Hosting;

namespace SlipeServer.Hosting;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureMtaServers(this IHostBuilder host, Action<IMtaServersConfiguration> configureAction)
    {
        var configuration = new MtaServersConfiguration(host);
        configureAction(configuration);
        return host;
    }
}
