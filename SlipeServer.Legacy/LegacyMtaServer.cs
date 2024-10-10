using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SlipeServer.Server.Resources.Serving;
using SlipeServer.ConfigurationProviders.Configurations;
using SlipeServer.Lua;
using SlipeServer.Server.Resources.Interpreters;
using Microsoft.Extensions.Logging.Console;
using SlipeServer.Server.Resources;

namespace SlipeServer.Legacy;

public class LegacyMtaServer
{
    private readonly IHostBuilder hostBuilder;
    private readonly Configuration configuration;

    public LegacyMtaServer()
    {
        this.hostBuilder = Host.CreateDefaultBuilder();

        var mtaServerConfiguration = new XmlConfigurationProvider("mods/deathmatch/mtaserver.conf");

        this.configuration = mtaServerConfiguration.Configuration;

        this.hostBuilder.ConfigureServices((hostBuilderContext, services) =>
        {
            if (hostBuilderContext.HostingEnvironment.IsDevelopment())
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var projectDirectory = Directory.GetParent(currentDirectory).Parent.Parent.FullName;

                this.configuration.ResourceDirectory = Path.Join(projectDirectory, "mods/deathmatch/resources");
            } else
            {
                this.configuration.ResourceDirectory = "mods/deathmatch/resources";
            }
            services.AddLogging(configure =>
            {
                configure.AddConsole(options =>
                {
                    options.FormatterName = "customFormatter";
                });
                configure.AddConsoleFormatter<CustomConsoleFormatter, SimpleConsoleFormatterOptions>();
            });

            services.AddSingleton<InteractiveServerConsole>();
            services.AddSingleton<ClientResourceService>();
            services.AddLua();
            services.AddHttpClient();
            services.AddDefaultMtaServerServices();
            services.AddResourceInterpreter<MetaXmlResourceInterpreter>();

            services.AddSingleton<IResourceServer, BasicHttpServer>();
            services.AddHostedService<LegacyServerService>();
            services.AddHostedService<LegacyConsoleCommandsService>();

            services.TryAddSingleton<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
        });

        this.hostBuilder.AddMtaServer<Player>(serverBuilder =>
        {
            serverBuilder.UseConfiguration(configuration!);
            serverBuilder.AddHostedDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
        });
    }

    public async Task BuildAndRunAsync(CancellationToken cancellationToken = default)
    {
        var app = this.hostBuilder.Build();
        Console.WriteLine("==================================================================");
        Console.WriteLine("= Multi Theft Auto: San Andreas v1.6");
        Console.WriteLine("= Server name      : {0}", this.configuration.ServerName);
        Console.WriteLine("= Server IP address: {0}", this.configuration.Host);
        Console.WriteLine("= Server port      : {0}", this.configuration.Port);
        Console.WriteLine("=");
        Console.WriteLine("= Log file         : {0}", "none");
        Console.WriteLine("= Maximum players  : {0}", this.configuration.MaxPlayerCount);
        Console.WriteLine("= HTTP port        : {0}", this.configuration.HttpPort);
        Console.WriteLine("= Voice Chat       : {0}", this.configuration.IsVoiceEnabled ? "Enabled" : "Disabled");
        Console.WriteLine("= Bandwidth saving : {0}", "TODO");
        Console.WriteLine("==================================================================");

        var serverConsole = app.Services.GetRequiredService<InteractiveServerConsole>();

        var _ = Task.Run(() =>
        {
            try
            {
                while (true)
                {
                    try
                    {
                        var line = Console.ReadLine();
                        if(line != null)
                        {
                            serverConsole.ExecuteCommand(line);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch(Exception ex)
            {

            }
        });
        await app.RunAsync(cancellationToken);
    }
}
