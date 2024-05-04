using SlipeServer.Server.Resources.Serving;

namespace SlipeServer.Hosting;

public sealed class ResourcesServerHostedService : IHostedService
{
    private readonly IResourceServer httpServer;

    public ResourcesServerHostedService(IResourceServer httpServer)
    {
        this.httpServer = httpServer;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.httpServer.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.httpServer.Stop();
        return Task.CompletedTask;
    }
}
