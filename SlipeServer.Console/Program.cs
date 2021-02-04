using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.ConfigurationProviders;
using SlipeServer.ConfigurationProviders.Configurations;
using SlipeServer.Lua;
using SlipeServer.Server;
using SlipeServer.Server.AllSeeingEye;
using SlipeServer.Server.Behaviour;
using SlipeServer.Server.PacketHandling.QueueHandlers;
using System;
using System.IO;
using System.Threading;

namespace SlipeServer.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Program(args);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
            }
        }

        private readonly EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private readonly MtaServer server;

        public Program(string[] args)
        {
            var configurationProvider = args.Length > 0 ? GetConfigurationProvider(args[0]) : null;

            Configuration? configuration = configurationProvider?.GetConfiguration();
            server = new MtaServer(
                Directory.GetCurrentDirectory(),
                @"net.dll",
                configuration,
                Configure
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
                server.Stop();
                waitHandle.Set();
            };

            SetupQueueHandlers();
            SetupBehaviour();
            SetupLogic();

            server.Start();
            
            waitHandle.WaitOne();
        }

        private void Configure(ServiceCollection services)
        {
            // Register additional services here to be injected into instances created with server.Instantiate()
            services.AddSingleton<ILogger, ConsoleLogger>();

            services.AddLua();
        }

        private IConfigurationProvider GetConfigurationProvider(string configPath)
        {
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Configuration file {configPath} does not exist.");
            }

            string extension = Path.GetExtension(configPath);
            switch (extension)
            {
                case ".json":
                    return new JsonConfigurationProvider(configPath);
                case ".xml":
                    return new XmlConfigurationProvider(configPath);
                case ".toml":
                    return new TomlConfigurationProvider(configPath);
                default:
                    throw new NotSupportedException($"Unsupported configuration extension {extension}");
            }
        }

        private void SetupQueueHandlers()
        {
            server.RegisterPacketQueueHandler<ConnectionQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler<RpcQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler<SyncQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler<CommandQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler<LuaEventQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler<PlayerEventQueueHandler>(10, 1);
        }

        private void SetupBehaviour()
        {
            server.Instantiate<AseBehaviour>();
            server.Instantiate<MasterServerAnnouncementBehaviour>("http://master.mtasa.com/ase/add.php");

            server.Instantiate<EventLoggingBehaviour>();
            server.Instantiate<VelocityBehaviour>();
            server.Instantiate<DefaultChatBehaviour>();
            server.Instantiate<NicknameChangeBehaviour>();

            server.Instantiate<PlayerJoinElementBehaviour>();

            server.Instantiate<ElementPacketBehaviour>();
            server.Instantiate<PedPacketBehaviour>();
            server.Instantiate<PlayerPacketBehaviour>();
        }

        private void SetupLogic()
        {
            this.server.Instantiate<ServerTestLogic>();
            this.server.Instantiate<LuaTestLogic>();
        }
    }
}
