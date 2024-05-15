namespace SlipeServer.Hosting;

public interface IMtaServersConfiguration
{
    void AddDefaultPacketHandlers(ServerBuilderDefaultPacketHandlers except = ServerBuilderDefaultPacketHandlers.None);
    void AddDefaultBehaviours(ServerBuilderDefaultBehaviours except = ServerBuilderDefaultBehaviours.None);

    /// <summary>
    /// Configure that all mta servers will start on application startup
    /// </summary>
    void StartAllServers();
}

internal sealed class MtaServersConfiguration : IMtaServersConfiguration
{
    private readonly IServiceCollection services;

    public MtaServersConfiguration(IServiceCollection services)
    {
        this.services = services;
    }

    public void StartAllServers()
    {
        this.services.AddHostedService<DefaultStartAllMtaServersHostedService>();
    }

    public void AddDefaultPacketHandlers(ServerBuilderDefaultPacketHandlers except = ServerBuilderDefaultPacketHandlers.None)
    {
        this.services.AddHostedService(x => new AddDefaultPacketHandlersHostedService(x.GetRequiredService<IEnumerable<MtaServer>>(), except));
    }

    public void AddDefaultBehaviours(ServerBuilderDefaultBehaviours except = ServerBuilderDefaultBehaviours.None)
    {
        this.services.AddHostedService(x => new AddDefaultBehavioursHostedService(x.GetRequiredService<IEnumerable<MtaServer>>(), except));
    }
}
