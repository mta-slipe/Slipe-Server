namespace SlipeServer.Hosting;

public interface IMtaServersConfiguration
{
    HostBuilderContext HostBuilderContext { get; }

    void AddDefaultPacketHandlers(ServerBuilderDefaultPacketHandlers except = ServerBuilderDefaultPacketHandlers.None);
    void AddDefaultBehaviours(ServerBuilderDefaultBehaviours except = ServerBuilderDefaultBehaviours.None);

    /// <summary>
    /// Configure that all mta servers will start on application startup
    /// </summary>
    void StartAllServers();
    void StartResourceServers();
}

internal sealed class MtaServersConfiguration : IMtaServersConfiguration
{
    private readonly HostBuilderContext builderContext;
    private readonly IServiceCollection services;

    public HostBuilderContext HostBuilderContext => this.builderContext;

    public MtaServersConfiguration(HostBuilderContext builderContext, IServiceCollection services)
    {
        this.builderContext = builderContext;
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

    public void StartResourceServers()
    {
        this.services.AddHostedService<ResourcesServerHostedService>();
    }
}
