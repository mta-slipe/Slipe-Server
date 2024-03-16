using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlipeServer.ConfigurationProviders;
using SlipeServer.Example;
using SlipeServer.Example.Elements;
using SlipeServer.Example.Logic;
using SlipeServer.Example.Proxy;
using SlipeServer.Lua;
using SlipeServer.LuaControllers;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Physics.Extensions;
using SlipeServer.Server;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ServerBuilders;
using System;
using System.IO;
using System.Threading;

namespace SlipeServer.Console;

public partial class Program
{
    public static void Main(string[] args)
    {
        var hostingType = HostingType.HostBuilder;
        Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

        Program? program = null;
        try
        {
            program = new Program(args, hostingType);
            program.Start(hostingType);
        }
        catch (Exception exception)
        {
            if (program != null)
            {
                program.Logger.LogCritical(exception, "{message}\n{stackTrace}", exception.Message, exception.StackTrace);
            } else
            {
                System.Console.WriteLine($"Error in startup {exception.Message}");
                System.Console.WriteLine($"Error in startup {exception.Message}\n{exception.StackTrace}");
            }

#if DEBUG
            throw;
#else
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
#endif
        }
    }

    private readonly EventWaitHandle waitHandle = new(false, EventResetMode.AutoReset);
    private readonly IHost? host;
    private readonly MtaServer server;
    private readonly Configuration configuration;

    public ILogger Logger { get; }

    public Program(string[] args, HostingType hostingType)
    {
        var configurationProvider = args.Length > 0 ? ConfigurationLoader.GetConfigurationProvider(args[0]) : null;

        this.configuration = configurationProvider?.GetConfiguration() ?? new Configuration()
        {
            IsVoiceEnabled = true,
#if DEBUG
            DebugPort = 50667
#endif
        };

        if (hostingType == HostingType.HostBuilder)
        {
            this.host = CreateHostBuilder(args).Build();
            this.server = this.host.Services.GetRequiredService<MtaServer<CustomPlayer>>();
        } else if (hostingType == HostingType.Standalone)
        {
            this.server = MtaServer.CreateWithDiSupport<CustomPlayer>(builder =>
            {
                builder.ConfigureExampleServer(this.configuration);
            });
        } else
            throw new NotSupportedException();

        this.server.GameType = "Slipe Server";
        this.server.MapName = "N/A";

        this.Logger = this.server.GetRequiredService<ILogger>();

        System.Console.CancelKeyPress += (sender, args) =>
        {
            this.server.Stop();
            this.waitHandle.Set();
        };
    }

    public void Start(HostingType hostingType)
    {
        if(hostingType == HostingType.HostBuilder)
        {
            this.host!.Run();
        }
        else if(hostingType == HostingType.Standalone)
        {
            this.server.Start();
            this.Logger.LogInformation("Server started.");
            this.waitHandle.WaitOne();
        }
    }

    public HostApplicationBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<ILogger>(x => x.GetRequiredService<ILogger<Program>>());

        builder.ConfigureMtaServer<CustomPlayer>(builder =>
        {
            builder.ConfigureExampleServer(this.configuration);
        });

        return builder;
    }

    public enum HostingType
    {
        HostBuilder,
        Standalone
    }

}
