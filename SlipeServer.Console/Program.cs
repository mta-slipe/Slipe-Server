using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SlipeServer.ConfigurationProviders;
using SlipeServer.Console.AdditionalResources;
using SlipeServer.Console.Elements;
using SlipeServer.Console.Logic;
using SlipeServer.Console.PacketReplayer;
using SlipeServer.Console.Proxy;
using SlipeServer.Console.Services;
using SlipeServer.Lua;
using SlipeServer.LuaControllers;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Physics.Extensions;
using SlipeServer.Server;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Loggers;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ServerBuilders;
using System;
using System.Threading;

namespace SlipeServer.Console;

public partial class Program
{
    public static void Main(string[] args)
    {
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

#if false
        this.server = MtaServer.CreateWithDiSupport<CustomPlayer>(x => BuildServer(x, false));
#else
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMtaServer<CustomPlayer>(x => BuildServer(x, true), this.configuration);
        ConfigureServices(serviceCollection);
        serviceCollection.AddSingleton<Func<string>>(() => "External service collection.");
        var serviceProvider = serviceCollection.BuildServiceProvider();
        this.server = serviceProvider.GetRequiredService<MtaServer<CustomPlayer>>();
#endif

        this.server.GameType = "Slipe Server";
        this.server.MapName = "N/A";

        this.Logger = this.server.GetRequiredService<ILogger>();

        System.Console.CancelKeyPress += (sender, args) =>
        {
            this.server.Stop();
            this.waitHandle.Set();
        };
    }

    private void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<ISyncHandlerMiddleware<PlayerPureSyncPacket>, SubscriptionSyncHandlerMiddleware<PlayerPureSyncPacket>>();
        services.AddSingleton<ISyncHandlerMiddleware<KeySyncPacket>, SubscriptionSyncHandlerMiddleware<KeySyncPacket>>();

        services.AddScoped<TestService>();
        services.AddSingleton<PacketReplayerService>();
    }

    private void BuildServer(ServerBuilder builder, bool usingExternalServiceCollection)
    {
        builder.UseConfiguration(this.configuration);

#if DEBUG
        builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
#else
        builder.AddDefaults();
#endif

        if(!usingExternalServiceCollection)
            builder.ConfigureServices(ConfigureServices);

        builder.AddLua();
        builder.AddPhysics();
        builder.AddParachuteResource();
        builder.AddLuaControllers();

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
        builder.AddLogic(typeof(TestLogic));
        //builder.AddBehaviour<VelocityBehaviour>();
        //builder.AddBehaviour<EventLoggingBehaviour>();

        builder.AddPacketHandler<KeySyncReplayerPacketHandler, KeySyncPacket>();
        builder.AddPacketHandler<PureSyncReplayerPacketHandler, PlayerPureSyncPacket>();
        builder.AddLogic<PacketReplayerLogic>();
    }

    public void Start()
    {
        this.server.Start();
        this.Logger.LogInformation("Server started.");
        this.waitHandle.WaitOne();
    }
}
