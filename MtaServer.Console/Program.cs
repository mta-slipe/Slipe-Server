using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Definitions.Player;
using MtaServer.Packets.Definitions.Sync;
using MtaServer.Packets.Definitions.Commands;
using MtaServer.Packets.Enums;
using MtaServer.Packets.Lua.Camera;
using MtaServer.Server.Elements;
using MtaServer.Server.PacketHandling.Factories;
using MtaServer.Server.PacketHandling.QueueHandlers;
using MtaServer.Server.Repositories;
using MtaServer.Server.Behaviour;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace MtaServer.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            new Program();
        }

        private readonly Server.MtaServer server;

        public Program()
        {
            server = new Server.MtaServer(Directory.GetCurrentDirectory(), @"net.dll", "0.0.0.0", 50666, new CompoundElementRepository());

            SetupQueueHandlers();
            SetupTestLogic();

            new DefaultChatBehaviour(server);

            server.Start();
            Thread.Sleep(-1);
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
            Client.OnJoin += (client) =>
            {
                System.Console.WriteLine($"{client.Name} ({client.Version}) ({client.Serial}) has joined the server!");
                client.SendPacket(new SetCameraTargetPacket(client.Id));
                client.SendPacket(new SpawnPlayerPacket(
                    client.Id,
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
