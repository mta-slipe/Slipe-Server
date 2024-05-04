using Microsoft.Extensions.DependencyInjection.Extensions;
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
    configure.StartAllServers();
});

var app = builder.Build();

await app.RunAsync();
