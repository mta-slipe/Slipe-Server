using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SlipeServer.Server.Resources.Serving;


Configuration? configuration = null;

var builder = Host.CreateDefaultBuilder(args);
builder
    .ConfigureServices((hostBuilderContext, services) =>
    {
        configuration = hostBuilderContext.Configuration.GetRequiredSection("MtaServer").Get<Configuration>();

        services.AddHttpClient();
        services.AddSingleton<IResourceServer, BasicHttpServer>();

        services.AddHostedService<SampleHostedService>(); // Use instead of logics
        services.TryAddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
    })
    .AddMtaServer(builder =>
    {
        builder.UseConfiguration(configuration!);
        builder.AddHostedDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
    });

var app = builder.Build();

await app.RunAsync();
