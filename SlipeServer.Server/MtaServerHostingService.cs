using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SlipeServer.Server;

internal sealed class MtaServerHostingService : IHostedService
{
    private readonly IEnumerable<MtaServer> servers;
    private readonly ILogger logger;

    public MtaServerHostingService(IEnumerable<MtaServer> servers, ILogger logger)
    {
        this.servers = servers;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var count = this.servers.Count();
        foreach (var item in this.servers)
        {
            item.Start();
        }

        if (count == 1)
        {
            this.logger.LogInformation("Mta server started.");
        } else
        {
            this.logger.LogInformation("Started {serverCount} mta servers.", count);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Server stopped.");
        return Task.CompletedTask;
    }
}
