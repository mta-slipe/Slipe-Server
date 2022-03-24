using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.ConfigurationProviders;
using SlipeServer.Console.Logic;
using SlipeServer.Lua;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Physics.Extensions;
using SlipeServer.Server;
using SlipeServer.Server.PacketHandling.Handlers.Middleware;
using SlipeServer.Server.ServerOptions;
using System;
using System.Threading;

namespace SlipeServer.Console
{
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
                    program.Logger.LogCritical(exception, exception.Message);
                } else
                {
                    System.Console.WriteLine($"Error in startup {exception.Message}");
                }
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
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
                IsVoiceEnabled = true
            };

            this.server = new MtaServer(
                (builder) =>
                {
                    builder.UseConfiguration(this.configuration);

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
                    });
                    builder.AddLua();
                    builder.AddPhysics();

                    builder.AddLogic<ServerTestLogic>();
                    builder.AddLogic<LuaTestLogic>();
                    builder.AddLogic<PhysicsTestLogic>();
                }
            )
            {
                GameType = "Slipe Server",
                MapName = "N/A"
            };

            this.Logger = this.server.GetRequiredService<ILogger>();

            System.Console.CancelKeyPress += delegate {
                this.server.Stop();
                this.waitHandle.Set();
            };
        }

        public void Start()
        {
            this.server.Start();
            this.waitHandle.WaitOne();
        }
    }
}
