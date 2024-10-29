using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SlipeServer.ConfigurationProviders;
using SlipeServer.Console.AdditionalResources;
using SlipeServer.Console.Logic;
using SlipeServer.Console.PacketReplayer;
using SlipeServer.Console.Proxy;
using SlipeServer.Console.Services;
using SlipeServer.Lua;
using SlipeServer.LuaControllers;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Physics.Extensions;
using SlipeServer.Server;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.Loggers;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ServerBuilders;
using System;
using System.IO;
using System.Threading;
using SlipeServer.Example;

namespace SlipeServer.Console;

public partial class Program
{
    public static void Main(string[] args)
    {
        Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

        Program? program = null;
        try
        {
            program = new Program(args);
            program.Start();
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
    private readonly MtaServer server;
    private readonly Configuration configuration;

    public ILogger Logger { get; }

    public Program(string[] args)
    {
        var configurationProvider = args.Length > 0 ? ConfigurationLoader.GetConfigurationProvider(args[0]) : null;

        this.configuration = configurationProvider?.GetConfiguration() ?? new Configuration()
        {
            IsVoiceEnabled = true,
#if DEBUG
            DebugPort = 50667
#endif
        };

        IConfigurationRoot config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        if(ushort.TryParse(config.GetSection("HttpPort").Value, out var httpPort))
        {
            this.configuration.HttpPort = httpPort;
        }

        this.server = MtaServer.CreateWithDiSupport<CustomPlayer>(
            (builder) =>
            {
                builder.UseConfiguration(this.configuration);

#if DEBUG
                builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
#else
                builder.AddDefaults();
#endif

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<ISyncHandlerMiddleware<PlayerPureSyncPacket>, SubscriptionSyncHandlerMiddleware<PlayerPureSyncPacket>>();
                    services.AddSingleton<ISyncHandlerMiddleware<KeySyncPacket>, SubscriptionSyncHandlerMiddleware<KeySyncPacket>>();

                    services.AddScoped<TestService>();
                    services.AddSingleton<PacketReplayerService>();
                    services.AddScoped<SampleScopedService>();

                    services.AddHttpClient();

                });
                builder.AddLua();
                builder.AddPhysics();
                builder.AddParachuteResource();
                builder.AddLuaControllers();

                builder.AddExampleLogic();
                builder.AddLogic<ServerTestLogic>();
                builder.AddLogic<LuaTestLogic>();
                builder.AddLogic<PhysicsTestLogic>();
                builder.AddLogic<ElementPoolingTestLogic>();
                builder.AddLogic<WarpIntoVehicleLogic>();
                builder.AddLogic<LuaEventTestLogic>();
                builder.AddLogic<ServiceUsageTestLogic>();
                builder.AddLogic<NametagTestLogic>();
                builder.AddLogic<VehicleTestLogic>();
                builder.AddLogic<ClothingTestLogic>();
                builder.AddLogic<PedTestLogic>();
                builder.AddLogic<ProxyService>();
                builder.AddLogic<LatentPacketTestLogic>();
                builder.AddScopedLogic<ScopedTestLogic1>();
                builder.AddScopedLogic<ScopedTestLogic2>();
                builder.AddLogic<VehicleEntityAddTestLogic>();
                builder.AddLogic(typeof(TestLogic));
                builder.AddLogic(typeof(NotVisibleToAllTestLogic));
                //builder.AddBehaviour<VelocityBehaviour>();
                builder.AddBehaviour<EventLoggingBehaviour>();

                builder.AddPacketHandler<KeySyncReplayerPacketHandler, KeySyncPacket>();
                builder.AddPacketHandler<PureSyncReplayerPacketHandler, PlayerPureSyncPacket>();
                builder.AddLogic<PacketReplayerLogic>();
            }
        );

        this.server.GameType = "Slipe Server";
        this.server.MapName = "N/A";

        this.Logger = this.server.GetRequiredService<ILogger>();

        System.Console.CancelKeyPress += (sender, args) =>
        {
            this.server.Stop();
            this.waitHandle.Set();
        };
    }

    public void Start()
    {
        this.server.Start();
        this.Logger.LogInformation("Server started.");
        this.waitHandle.WaitOne();
    }
}
