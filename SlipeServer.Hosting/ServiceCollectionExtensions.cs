using SlipeServer.Server;

namespace SlipeServer.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMtaServer<T>(this IServiceCollection services, T mtaServer, Action<ServerBuilder>? builderAction = null)
        where T : MtaServer
    {
        Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

        if (builderAction != null)
            services.AddHostedService(x => new BuildServerHostedService(x.GetRequiredService<IEnumerable<MtaServer>>(), builderAction));

        services.AddSingleton(mtaServer);
        services.AddSingleton<MtaServer>(x => mtaServer);
        services.AddSingleton<Configuration>(x => x.GetRequiredService<MtaServer>().Configuration);
        services.AddSingleton<RootElement>(x => x.GetRequiredService<MtaServer>().RootElement);
        return services;
    }

    public static IServiceCollection AddMtaServer<T>(this IServiceCollection services, Configuration configuration, Func<IServiceProvider, T> serverFactory, Action<ServerBuilder>? builderAction = null)
        where T : MtaServer
    {
        Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

        if (builderAction != null)
            services.AddHostedService(x => new BuildServerHostedService(x.GetRequiredService<IEnumerable<MtaServer>>(), builderAction));

        services.AddSingleton(serverFactory);
        services.AddSingleton<MtaServer>(x => x.GetRequiredService<T>());

        services.AddSingleton<Configuration>(x => x.GetRequiredService<MtaServer>().Configuration);
        services.AddSingleton<RootElement>(x => x.GetRequiredService<MtaServer>().RootElement);
        return services;
    }

    public static IServiceCollection AddMtaServerWithDIPlaer<T>(this IServiceCollection services, Configuration configuration, Action<ServerBuilder>? builderAction = null)
        where T : Player
    {
        Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

        services.AddSingleton(services => new MtaDiPlayerServer<T>(services, configuration));

        if(builderAction != null)
            services.AddHostedService(x => new BuildServerHostedService(x.GetRequiredService<IEnumerable<MtaServer>>(), builderAction));

        services.AddSingleton<MtaServer<T>>(x => x.GetRequiredService<MtaDiPlayerServer<T>>());
        services.AddSingleton<MtaServer>(x => x.GetRequiredService<MtaDiPlayerServer<T>>());
        services.AddSingleton<Configuration>(x => x.GetRequiredService<MtaServer>().Configuration);
        services.AddSingleton<RootElement>(x => x.GetRequiredService<MtaServer>().RootElement);
        return services;
    }
}

public sealed class BuildServerHostedService : IHostedService
{
    private readonly IEnumerable<MtaServer> mtaServers;
    private readonly Action<ServerBuilder> actionBuilder;

    public BuildServerHostedService(IEnumerable<MtaServer> mtaServers, Action<ServerBuilder> actionBuilder)
    {
        this.mtaServers = mtaServers;
        this.actionBuilder = actionBuilder;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var builder = new ServerBuilder();
        this.actionBuilder(builder);

        foreach (var mtaServer in mtaServers)
        {
            builder.ApplyTo(mtaServer);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
