namespace SlipeServer.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMtaServer<T>(this IServiceCollection services, T mtaServer)
        where T : MtaServer
    {
        services.AddSingleton(mtaServer);
        services.AddSingleton<MtaServer>(x => mtaServer);
        return services;
    }

    public static IServiceCollection AddMtaServer<T>(this IServiceCollection services, Configuration configuration, Action<ServerBuilder>? builderAction = null)
        where T : Player
    {
        Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

        services.AddSingleton((IServiceProvider services) => new MtaDiPlayerServer<T>(services, configure =>
        {
            configure.UseConfiguration(configuration);
            builderAction?.Invoke(configure);
        }));
        services.AddSingleton<MtaServer<T>>(x => x.GetRequiredService<MtaDiPlayerServer<T>>());
        services.AddSingleton<MtaServer>(x => x.GetRequiredService<MtaDiPlayerServer<T>>());
        services.AddSingleton<Configuration>(x => x.GetRequiredService<MtaServer>().Configuration);
        services.AddSingleton<RootElement>(x => x.GetRequiredService<MtaServer>().RootElement);
        return services;
    }
}
