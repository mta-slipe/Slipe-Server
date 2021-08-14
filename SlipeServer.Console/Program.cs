using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.ConfigurationProviders;
using SlipeServer.ConfigurationProviders.Configurations;
using SlipeServer.Lua;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Server;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware;
using SlipeServer.Server.Repositories;
using System;
using System.IO;
using System.Threading;

namespace SlipeServer.Console
{
    public class Program
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
            this.Logger = new ConsoleLogger();

            var configurationProvider = args.Length > 0 ? GetConfigurationProvider(args[0]) : null;

            this.configuration = (configurationProvider?.GetConfiguration() ?? new Configuration()
            {
                IsVoiceEnabled = true
            });
            this.server = new MtaServer(
                Directory.GetCurrentDirectory(),
                @"net.dll",
                this.configuration,
                this.Configure
            )
            {
                GameType = "Slipe Server",
                MapName = "N/A"
            };

#if DEBUG
            server.AddNetWrapper(
                Directory.GetCurrentDirectory(),
                @"net_d.dll",
                configuration?.Host ?? "0.0.0.0",
                (ushort)((configuration?.Port + 1) ?? 50667)
            );
#endif

            System.Console.CancelKeyPress += delegate
            {
                this.server.Stop();
                this.waitHandle.Set();
            };
        }

        public void Start()
        {
            SetupQueueHandlers();
            SetupBehaviour();
            SetupLogic();

            this.server.Start();
            this.waitHandle.WaitOne();
        }

        private void Configure(ServiceCollection services)
        {
            services.AddSingleton<ILogger>(this.Logger);
            services.AddSingleton<ISyncHandlerMiddleware<ProjectileSyncPacket>, RangeSyncHandlerMiddleware<ProjectileSyncPacket>>(
                x => new RangeSyncHandlerMiddleware<ProjectileSyncPacket>(x.GetRequiredService<IElementRepository>(), this.configuration.ExplosionSyncDistance)    
            );

            services.AddLua();
        }

        private IConfigurationProvider GetConfigurationProvider(string configPath)
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Configuration file {configPath} does not exist.");
            }

            string extension = Path.GetExtension(configPath);
            return extension switch
            {
                ".json" => new JsonConfigurationProvider(configPath),
                ".xml" => new XmlConfigurationProvider(configPath),
                ".toml" => new TomlConfigurationProvider(configPath),
                _ => throw new NotSupportedException($"Unsupported configuration extension {extension}"),
            };
        }

        private void SetupQueueHandlers()
        {
            this.server.RegisterPacketQueueHandler<ExplosionSyncQueueHandler>(10, 1);
            this.server.RegisterPacketQueueHandler<ConnectionQueueHandler>(10, 1);
            this.server.RegisterPacketQueueHandler<RpcQueueHandler>(10, 1);
            this.server.RegisterPacketQueueHandler<SyncQueueHandler>(QueueHandlerScalingConfig.Aggressive, 10);
            this.server.RegisterPacketQueueHandler<CommandQueueHandler>(10, 1);
            this.server.RegisterPacketQueueHandler<LuaEventQueueHandler>(10, 1);
            this.server.RegisterPacketQueueHandler<PlayerEventQueueHandler>(10, 1);
            this.server.RegisterPacketQueueHandler<VehicleInOutHandler>(10, 1);
            this.server.RegisterPacketQueueHandler<VehicleSyncQueueHandler>(QueueHandlerScalingConfig.Aggressive, 10);
            this.server.RegisterPacketQueueHandler<VoiceHandler>(10, 1);
        }

        private void SetupBehaviour()
        {
            this.server.Instantiate<AseBehaviour>();
            this.server.Instantiate<MasterServerAnnouncementBehaviour>("http://master.mtasa.com/ase/add.php");

            this.server.Instantiate<EventLoggingBehaviour>();
            this.server.Instantiate<VelocityBehaviour>();
            this.server.Instantiate<DefaultChatBehaviour>();
            this.server.Instantiate<NicknameChangeBehaviour>();

            this.server.Instantiate<PlayerJoinElementBehaviour>();

            this.server.Instantiate<ElementPacketBehaviour>();
            this.server.Instantiate<PedPacketBehaviour>();
            this.server.Instantiate<PlayerPacketBehaviour>();
            this.server.Instantiate<VehicleWarpBehaviour>();
            this.server.Instantiate<VoiceBehaviour>();
        }

        private void SetupLogic()
        {
            this.server.Instantiate<ServerTestLogic>();
            this.server.Instantiate<LuaTestLogic>();
        }
    }
}
