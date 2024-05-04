﻿using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.Mappers;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    var configurationProvider = args.Length > 0 ? ConfigurationLoader.GetConfigurationProvider(args[0]) : null;

    var configuration = configurationProvider?.GetConfiguration() ?? new Configuration()
    {
        IsVoiceEnabled = true,
#if DEBUG
        DebugPort = 50667
#endif
    };

    services.AddHttpClient();
    services.AddDefaultMtaServerServices();
    services.AddMtaServer<CustomPlayer>(configuration, builder =>
    {
        builder.AddDefaultServices();
        builder.AddDefaultLuaMappings();
        builder.AddDefaultNetWrapper();
    });

    services.AddSingleton<IResourceServer, BasicHttpServer>();
    services.AddHostedService<ResourcesServerHostedService>();

    services.AddHostedService<SampleHostedService>(); // Use instead of logics
    services.TryAddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
});

builder.ConfigureMtaServers(configure =>
{
    var isDevelopment = configure.HostBuilderContext.HostingEnvironment.IsDevelopment();
    var exceptBehaviours = isDevelopment ? ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour : ServerBuilderDefaultBehaviours.None;

    configure.AddDefaultPacketHandlers();
    configure.AddDefaultBehaviours(exceptBehaviours);
    configure.StartResourceServers();
    configure.StartAllServers();
});


var app = builder.Build();

await app.RunAsync();
