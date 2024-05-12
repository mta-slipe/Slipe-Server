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
        foreach (var mtaServer in this.mtaServers)
        {
            mtaServer.Start();
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var mtaServer in this.mtaServers)
        {
            mtaServer.Stop();
        }
        return Task.CompletedTask;
    }
}

public sealed class AddDefaultPacketHandlersHostedService : IHostedService
{
    private readonly IEnumerable<MtaServer> mtaServers;
    private readonly ServerBuilderDefaultPacketHandlers except;

    public AddDefaultPacketHandlersHostedService(IEnumerable<MtaServer> mtaServers, ServerBuilderDefaultPacketHandlers except = ServerBuilderDefaultPacketHandlers.None)
    {
        this.mtaServers = mtaServers;
        this.except = except;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var mtaServer in this.mtaServers)
        {
            mtaServer.AddDefaultPacketHandlers(this.except);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public sealed class AddDefaultBehavioursHostedService : IHostedService
{
    private readonly IEnumerable<MtaServer> mtaServers;
    private readonly ServerBuilderDefaultBehaviours except;

    public AddDefaultBehavioursHostedService(IEnumerable<MtaServer> mtaServers, ServerBuilderDefaultBehaviours except = ServerBuilderDefaultBehaviours.None)
    {
        this.mtaServers = mtaServers;
        this.except = except;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var mtaServer in this.mtaServers)
        {
            mtaServer.AddDefaultBehaviours(this.except);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
