namespace SlipeServer.Hosting;

public static class HostBuilderExtensions
{
    private static IServiceCollection AddMtaServerCore(this IServiceCollection services)
    {
        services.AddSingleton<Configuration>(x => x.GetRequiredService<MtaServer>().Configuration);
        services.AddSingleton<RootElement>(x => x.GetRequiredService<MtaServer>().RootElement);
        return services;
    }

    public static IHostBuilder AddMtaServer(this IHostBuilder host, Action<ServerBuilder> buildAction)
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddDefaultMtaServerServices();
            services.AddDefaultMiddlewares();

            services.AddSingleton<MtaServer>(x => new MtaServer(x, buildAction));
            services.AddHostedService<MtaServerHostedService<MtaServer>>();

            services.AddMtaServerCore();
        });

        return host;
    }

    public static IHostApplicationBuilder AddMtaServer(this IHostApplicationBuilder host, Action<ServerBuilder> buildAction)
    {
        host.Services.AddDefaultMtaServerServices();
        host.Services.AddDefaultMiddlewares();

        host.Services.AddSingleton<MtaServer>(x => new MtaServer(x, buildAction));
        host.Services.AddSingleton<IMtaServer>(x => x.GetRequiredService<MtaServer>());
        host.Services.AddHostedService<MtaServerHostedService<MtaServer>>();

        host.Services.AddMtaServerCore();

        return host;
    }


    public static IHostBuilder AddMtaServer<T>(this IHostBuilder host, Action<ServerBuilder> buildAction) where T : Player, new()
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddDefaultMtaServerServices();
            services.AddDefaultMiddlewares();

            services.AddSingleton<MtaServer<T>>(x => new MtaNewPlayerServer<T>(x, buildAction));
            services.AddSingleton<IMtaServer<T>>(x => x.GetRequiredService<MtaServer<T>>());
            services.AddSingleton<MtaServer>(x => x.GetRequiredService<MtaServer<T>>());
            services.AddSingleton<IMtaServer>(x => x.GetRequiredService<MtaServer>());
            services.AddHostedService<MtaServerHostedService<MtaServer>>();

            services.AddMtaServerCore();
        });

        return host;
    }

    public static IHostApplicationBuilder AddMtaServer<T>(this IHostApplicationBuilder host, Action<ServerBuilder> buildAction) where T : Player, new()
    {
        host.Services.AddDefaultMtaServerServices();
        host.Services.AddDefaultMiddlewares();

        host.Services.AddSingleton<MtaServer<T>>(x => new MtaNewPlayerServer<T>(x, buildAction));
        host.Services.AddSingleton<IMtaServer<T>>(x => x.GetRequiredService<MtaServer<T>>());
        host.Services.AddSingleton<MtaServer>(x => x.GetRequiredService<MtaServer<T>>());
        host.Services.AddSingleton<IMtaServer>(x => x.GetRequiredService<MtaServer>());
        host.Services.AddHostedService<MtaServerHostedService<MtaServer>>();

        host.Services.AddMtaServerCore();

        return host;
    }


    public static IHostBuilder AddMtaServerWithDiSupport<T>(this IHostBuilder host, Action<ServerBuilder> buildAction) where T : Player
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddDefaultMtaServerServices();
            services.AddDefaultMiddlewares();

            services.AddSingleton<MtaDiPlayerServer<T>>(x => new MtaDiPlayerServer<T>(x, buildAction));
            services.AddSingleton<IMtaServer<T>>(x => x.GetRequiredService<MtaServer<T>>());
            services.AddSingleton<MtaServer>(x => x.GetRequiredService<MtaDiPlayerServer<T>>()); ;
            services.AddSingleton<IMtaServer>(x => x.GetRequiredService<MtaServer>());
            services.AddHostedService<MtaServerHostedService<MtaServer>>();

            services.AddMtaServerCore();
        });

        return host;
    }

    public static IHostApplicationBuilder AddMtaServerWithDiSupport<T>(this IHostApplicationBuilder host, Action<ServerBuilder> buildAction) where T : Player
    {
        host.Services.AddDefaultMtaServerServices();
        host.Services.AddDefaultMiddlewares();

        host.Services.AddSingleton<MtaDiPlayerServer<T>>(x => new MtaDiPlayerServer<T>(x, buildAction));
        host.Services.AddSingleton<IMtaServer<T>>(x => x.GetRequiredService<MtaServer<T>>());
        host.Services.AddSingleton<MtaServer>(x => x.GetRequiredService<MtaDiPlayerServer<T>>());
        host.Services.AddSingleton<IMtaServer>(x => x.GetRequiredService<MtaServer>());
        host.Services.AddHostedService<MtaServerHostedService<MtaServer>>();

        host.Services.AddMtaServerCore();

        return host;
    }
    
    public static IHostApplicationBuilder AddCustomMtaServerWithDiSupport<T>(this IHostApplicationBuilder host, Func<IServiceProvider, T> factory) where T : MtaServer
    {
        host.Services.AddDefaultMtaServerServices();
        host.Services.AddDefaultMiddlewares();

        host.Services.AddSingleton<T>(x => factory(x));
        host.Services.AddSingleton<MtaServer>(x => x.GetRequiredService<T>());
        host.Services.AddSingleton<IMtaServer>(x => x.GetRequiredService<MtaServer>());
        host.Services.AddHostedService<MtaServerHostedService<MtaServer>>();

        host.Services.AddMtaServerCore();

        return host;
    }


    public static IHostBuilder AddMtaServer(this IHostBuilder host, MtaServer server) 
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddDefaultMtaServerServices();
            services.AddDefaultMiddlewares();

            services.AddSingleton<MtaServer>(server);
            services.AddSingleton<IMtaServer>(x => x.GetRequiredService<MtaServer>());
            services.AddHostedService<MtaServerHostedService<MtaServer>>();

            services.AddMtaServerCore();
        });

        return host;
    }

    public static IHostApplicationBuilder AddMtaServer(this IHostApplicationBuilder host, MtaServer server)
    {
        host.Services.AddDefaultMtaServerServices();
        host.Services.AddDefaultMiddlewares();

        host.Services.AddSingleton<MtaServer>(server);
        host.Services.AddSingleton<IMtaServer>(x => x.GetRequiredService<MtaServer>());
        host.Services.AddHostedService<MtaServerHostedService<MtaServer>>();

        host.Services.AddMtaServerCore();

        return host;
    }


    public static IHostBuilder AddMtaServer<T>(this IHostBuilder host, T server) where T: MtaServer
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddDefaultMtaServerServices();
            services.AddDefaultMiddlewares();

            services.AddSingleton<MtaServer>(server);
            services.AddSingleton<IMtaServer>(x => x.GetRequiredService<MtaServer>());
            services.AddSingleton<T>(server);
            services.AddHostedService<MtaServerHostedService<MtaServer>>();

            services.AddMtaServerCore();
        });

        return host;
    }

    public static IHostApplicationBuilder AddMtaServer<T>(this IHostApplicationBuilder host, T server) where T : MtaServer
    {
        host.Services.AddDefaultMtaServerServices();
        host.Services.AddDefaultMiddlewares();

        host.Services.AddSingleton<T>(server);
        host.Services.AddSingleton<MtaServer>(x => x.GetRequiredService<T>());
        host.Services.AddSingleton<IMtaServer>(x => x.GetRequiredService<MtaServer>());
        host.Services.AddHostedService<MtaServerHostedService<MtaServer>>();

        host.Services.AddMtaServerCore();

        return host;
    }
}
