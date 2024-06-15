namespace SlipeServer.Hosting;

public class MtaServerHostedService<T> : IHostedService where T : MtaServer
{
    private readonly T server;

    public MtaServerHostedService(T server)
    {
        this.server = server;

        server.BuildFinalizer?.Invoke();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.server.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.server.Stop();

        return Task.CompletedTask;
    }
}
