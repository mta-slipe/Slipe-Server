namespace SlipeServer.Hosting;

public sealed class DefaultStartAllMtaServersHostedService : IHostedService
{
    private readonly IEnumerable<MtaServer> mtaServers;

    public DefaultStartAllMtaServersHostedService(IEnumerable<MtaServer> mtaServers)
    {
        this.mtaServers = mtaServers;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var mtaServer in mtaServers)
        {
            mtaServer.Start();
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var mtaServer in mtaServers)
        {
            mtaServer.Stop();
        }
        return Task.CompletedTask;
    }
}
