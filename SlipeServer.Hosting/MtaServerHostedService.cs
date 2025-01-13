namespace SlipeServer.Hosting;

public class MtaServerHostedService<T> : IHostedLifecycleService where T : MtaServer
{
    private readonly T server;

    public MtaServerHostedService(T server)
    {
        this.server = server;

        this.server.BuildFinalizer?.Invoke();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        this.server.Start();

        return Task.CompletedTask;
    }

    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        this.server.Stop();

        return Task.CompletedTask;
    }
}
