using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.ConfigurationProviders;
using SlipeServer.Example.AdditionalResources;
using SlipeServer.Example.Elements;
using SlipeServer.Example.Logic;
using SlipeServer.Example.Services;
using SlipeServer.Lua;
using SlipeServer.LuaControllers;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Physics.Extensions;
using SlipeServer.Server;
using SlipeServer.Server.Loggers;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.Resources.Interpreters;
using SlipeServer.Server.ServerBuilders;
using SlipeServer.Web;
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
#endif
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
        }
    }

    private readonly EventWaitHandle waitHandle = new(false, EventResetMode.AutoReset);
    private readonly MtaServer<CustomPlayer> server;
    private readonly SlipeWebApi api;
    private readonly Configuration configuration;

    public ILogger Logger { get; }

    public Program(string[] args)
    {
        var configurationProvider = args.Length > 0 ? ConfigurationLoader.GetConfigurationProvider(args[0]) : null;

        this.configuration = configurationProvider?.GetConfiguration() ?? new Configuration()
        {
            IsVoiceEnabled = true
        };

        this.api = new SlipeWebApi();
        this.api.InitialiseServices();
        this.server = MtaServer.CreateWithDiSupport<CustomPlayer>(
            (builder) =>
            {
                builder.UseConfiguration(this.configuration);
                builder.UseServiceCollection(this.api.Services);
                builder.AddPostDependencyBuildStep(() =>
                {
                    this.api.Build();
                    if (this.api.ServiceProvider != null)
                        builder.UseServiceProvider(this.api.ServiceProvider);
                });

#if DEBUG
                builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
                builder.AddNetWrapper(dllPath: "net_d", port: (ushort)(this.configuration.Port + 1));
#else
                    builder.AddDefaults();
#endif

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<ILogger, ConsoleLogger>();
                    services.AddSingleton<ISyncHandlerMiddleware<PlayerPureSyncPacket>, SubscriptionSyncHandlerMiddleware<PlayerPureSyncPacket>>();
                    services.AddSingleton<ISyncHandlerMiddleware<KeySyncPacket>, SubscriptionSyncHandlerMiddleware<KeySyncPacket>>();

                    services.AddScoped<TestService>();
                });
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
                //builder.AddBehaviour<VelocityBehaviour>();
                //builder.AddBehaviour<EventLoggingBehaviour>();
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
        this.api.Run();
        this.waitHandle.WaitOne();
    }
}
