using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlipeServer.ConfigurationProviders;
using SlipeServer.ConfigurationProviders.Configurations;
using SlipeServer.Packets.Enums;
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
        private readonly Server.MtaServer server;

        public Program(string[] args)
        {
            var configurationProvider = args.Length > 0 ? GetConfigurationProvider(args[0]) : null;

            server = new MtaServer(
                Directory.GetCurrentDirectory(),
                @"net.dll",
                configurationProvider?.GetConfiguration(),
                Configure
            )
            {
                GameType = "Slipe Server",
                MapName = "N/A"
            };

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
            // Register additional services here to be injected into instances created with server.CreateInstance()
            services.AddSingleton<ILogger, ConsoleLogger>();
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
            ConnectionQueueHandler connectionQueueHandler = this.server.Instantiate<ConnectionQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_JOIN, connectionQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_JOINDATA, connectionQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_QUIT, connectionQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_TIMEOUT, connectionQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_NO_SOCKET, connectionQueueHandler);

            RpcQueueHandler rpcQueueHandler = this.server.Instantiate<RpcQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_RPC, rpcQueueHandler);

            SyncQueueHandler syncQueueHandler = this.server.Instantiate<SyncQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_CAMERA_SYNC, syncQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, syncQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_KEYSYNC, syncQueueHandler);

            CommandQueueHandler commandQueueHandler = this.server.Instantiate<CommandQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_COMMAND, commandQueueHandler);

            LuaEventQueueHandler luaEventQueueHandler = this.server.Instantiate<LuaEventQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_LUA_EVENT, luaEventQueueHandler);
        }

        private void SetupLogic()
        {
            this.server.Instantiate<ServerTestLogic>();
        }

        private void SetupBehaviour()
        {
            server.Instantiate<DefaultChatBehaviour>();
            server.Instantiate<BasicElementRepositoryBehaviour>();
            server.Instantiate<PlayerJoinElementBehaviour>();
            server.Instantiate<AseBehaviour>();
            server.Instantiate<MasterServerAnnouncementBehaviour>("http://master.mtasa.com/ase/add.php");
            server.Instantiate<EventLoggingBehaviour>();
        }
    }
}
