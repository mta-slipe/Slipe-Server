using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SlipeServer.Hosting;

public interface IMtaServersConfiguration
{
    /// <summary>
    /// Configure that all mta servers will start on application startup
    /// </summary>
    void StartAllServers();
}

internal sealed class MtaServersConfiguration : IMtaServersConfiguration
{
    private readonly IHostBuilder host;

    public MtaServersConfiguration(IHostBuilder host)
    {
        this.host = host;
    }

    public void StartAllServers()
    {
        this.host.ConfigureServices((hostBuilderContext, services) =>
        {
            services.AddHostedService<DefaultStartAllMtaServersHostedService>();
        });
    }
}
