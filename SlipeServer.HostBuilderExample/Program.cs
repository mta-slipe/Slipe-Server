using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Example;
using SlipeServer.Example.Services;
using SlipeServer.Example.Elements;


Configuration? configuration = null;

var builder = Host.CreateDefaultBuilder(args);
builder
    .ConfigureServices((hostBuilderContext, services) =>
    {
        configuration = hostBuilderContext.Configuration.GetRequiredSection("MtaServer").Get<Configuration>();

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        if (configuration != null && ushort.TryParse(config.GetSection("HttpPort").Value, out var httpPort))
        {
            configuration.HttpPort = httpPort;
        }

        services.AddHttpClient();
        services.AddSingleton<IResourceServer, BasicHttpServer>();
        services.AddScoped<TestService>();

        services.AddHostedService<SampleHostedService>(); // Use instead of logics
        services.TryAddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
    })
    .AddMtaServerWithDiSupport<CustomPlayer>(builder =>
    {
        builder.UseConfiguration(configuration!);
        builder.AddHostedDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
        builder.AddExampleLogic();
    });

var app = builder.Build();

await app.RunAsync();
