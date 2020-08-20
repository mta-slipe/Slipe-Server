using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtaServer.ConfigurationProviders;
using MtaServer.ConfigurationProviders.Configurations;
using MtaServer.Packets.Enums;
using MtaServer.Server.Behaviour;
using MtaServer.Server.PacketHandling.QueueHandlers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MtaServer.Console
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

        private readonly Server.MtaServer server;
        public Commands.Commands Commands { get; }
        public ConsoleHandler ConsoleHandler { get; }

        public Program(string[] args)
        {
            var configurationProvider = args.Length > 0 ? GetConfigurationProvider(args[0]) : null;

            server = new Server.MtaServer(
                Directory.GetCurrentDirectory(),
                @"net.dll",
                configurationProvider?.GetConfiguration(),
                Configure
            );

            ConsoleHandler = new ConsoleHandler(this);
            Commands = new Commands.Commands(this);
            Thread.Sleep(500);

            SetupQueueHandlers();
            SetupBehaviour();
            SetupLogic();

            Task.Run(async () =>
            {
                string line;
                while (true)
                {
                    line = System.Console.ReadLine();
                    ConsoleHandler.HandleConsoleInput(line);
                }
            });
            server.Start();
            Thread.Sleep(-1);
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

            RpcQueueHandler rpcQueueHandler = this.server.Instantiate<RpcQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_RPC, rpcQueueHandler);

            SyncQueueHandler syncQueueHandler = this.server.Instantiate<SyncQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_CAMERA_SYNC, syncQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, syncQueueHandler);

            CommandQueueHandler commandQueueHandler = this.server.Instantiate<CommandQueueHandler>(10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_COMMAND, commandQueueHandler);
        }

        private void SetupLogic()
        {
            this.server.Instantiate<ServerTestLogic>();
        }

        private void SetupBehaviour()
        {
            server.Instantiate<DefaultChatBehaviour>();
        }
    }
}
