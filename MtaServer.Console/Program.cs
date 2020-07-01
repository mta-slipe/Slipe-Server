using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Definitions.Player;
using MtaServer.Packets.Definitions.Sync;
using MtaServer.Packets.Definitions.Commands;
using MtaServer.Packets.Enums;
using MtaServer.Packets.Lua.Camera;
using MtaServer.Server;
using MtaServer.Server.Elements;
using MtaServer.Server.PacketHandling.Factories;
using MtaServer.Server.PacketHandling.QueueHandlers;
using MtaServer.Server.Repositories;
using MtaServer.Server.Behaviour;
using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using MtaServer.ConfigurationProviders;
using MtaServer.ConfigurationProviders.Configurations;
using MtaServer.Packets.Definitions.Lua.ElementRpc.Element;

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
        public Logic.Commands Commands { get; }
        public Logic.ConsoleHandler ConsoleHandler { get; }

        public Program(string[] args)
        {
            if (args.Length > 0)
            {
                IConfigurationProvider configurationProvider = GetConfiguration(args[0]);
                server = new Server.MtaServer(Directory.GetCurrentDirectory(), @"net.dll", new CompoundElementRepository(), configurationProvider.GetConfiguration());

            } else
            {
                server = new Server.MtaServer(Directory.GetCurrentDirectory(), @"net.dll", new CompoundElementRepository());
            }

            ConsoleHandler = new Logic.ConsoleHandler();
            Commands = new Logic.Commands(server, ConsoleHandler);
            Thread.Sleep(500);

            SetupConsole();
            SetupQueueHandlers();
            SetupTestLogic();

            new DefaultChatBehaviour(server);

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

        private IConfigurationProvider GetConfiguration(string configPath)
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

        private void SetupConsole()
        {
            ConsoleHandler.ConsoleOutput += message => System.Console.WriteLine(message);
        }

        private void SetupQueueHandlers()
        {
            ConnectionQueueHandler connectionQueueHandler = new ConnectionQueueHandler(server, 10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_JOIN, connectionQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_JOINDATA, connectionQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_QUIT, connectionQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_TIMEOUT, connectionQueueHandler);

            RpcQueueHandler rpcQueueHandler = new RpcQueueHandler(server, 10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_RPC, rpcQueueHandler);

            SyncQueueHandler syncQueueHandler = new SyncQueueHandler(server, 10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_CAMERA_SYNC, syncQueueHandler);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_PLAYER_PURESYNC, syncQueueHandler);

            CommandQueueHandler commandQueueHandler = new CommandQueueHandler(server, 10, 1);
            server.RegisterPacketQueueHandler(PacketId.PACKET_ID_COMMAND, commandQueueHandler);

        }

        private void SetupTestLogic()
        {
            Player.OnJoin += (player) =>
            {
                var client = player.Client;
                System.Console.WriteLine($"{player.Name} ({client.Version}) ({client.Serial}) has joined the server!");
                client.SendPacket(new SetCameraTargetPacket(player.Id));
                client.SendPacket(new SpawnPlayerPacket(
                    player.Id,
                    flags: 0,
                    position: new Vector3(0, 0, 3),
                    rotation: 0,
                    skin: 7,
                    teamId: 0,
                    interior: 0,
                    dimension: 0,
                    timeContext: 0
                ));
                client.SendPacket(new FadeCameraPacket(CameraFade.In));
                client.SendPacket(new ChatEchoPacket(server.Root.Id, "Hello World", Color.White));
                client.SendPacket(new ClearChatPacket());
                client.SendPacket(new ChatEchoPacket(server.Root.Id, "Hello World Again", Color.White));
                client.SendPacket(new ConsoleEchoPacket("Hello Console World"));

                TestPureSync(client);
            };
        }

        private void TestPureSync(Client client)
        {
            var playerList = new PlayerListPacket(false);
            playerList.AddPlayer(
                playerId: 666,
                timeContext: 0,
                nickname: "Dummy-Player",
                bitsreamVersion: 343,
                buildNumber: 0,

                isDead: false,
                isInVehicle: false,
                hasJetpack: true,
                isNametagShowing: true,
                isNametagColorOverriden: true,
                isHeadless: false,
                isFrozen: false,

                nametagText: "Dummy-Player",
                color: Color.FromArgb(255, 255, 0, 255),
                moveAnimation: 0,

                model: 9,
                teamId: null,

                vehicleId: null,
                seat: null,

                position: new Vector3(5, 0, 3),
                rotation: 0,

                dimension: 0,
                fightingStyle: 0,
                alpha: 255,
                interior: 0,

                weapons: new byte[16]
            );
            client.SendPacket(playerList);

            var data = new byte[] { 0, 0, 0, 0, 2, 46, 33, 240, 8, 159, 255, 240, 8, 4, 116, 11, 186, 246, 64, 0, 73, 144, 129, 19, 48, 0, 0 };
            var puresync = new PlayerPureSyncPacket();
            puresync.Read(data);

            puresync.PlayerId = 666;
            puresync.Latency = 0;

            _ = Task.Run(async () =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    puresync.Position += new Vector3(0.25f, 0, 0);
                    client.SendPacket(puresync);
                    await Task.Delay(250);
                }
            });
        }
    }
}
