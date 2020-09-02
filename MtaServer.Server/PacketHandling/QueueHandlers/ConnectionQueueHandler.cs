using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using MtaServer.Server.Enums;
using MtaServer.Server.PacketHandling.Factories;
using MtaServer.Server.Repositories;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MtaServer.Server.PacketHandling.QueueHandlers
{
    public class ConnectionQueueHandler : WorkerBasedQueueHandler
    {
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;

        public ConnectionQueueHandler(MtaServer server, IElementRepository elementRepository, int sleepInterval, int workerCount)
            : base(sleepInterval, workerCount)
        {
            this.server = server;
            this.elementRepository = elementRepository;
        }

        protected override void HandlePacket(PacketQueueEntry queueEntry)
        {
            switch (queueEntry.PacketId)
            {
                case PacketId.PACKET_ID_PLAYER_JOIN:
                    HandleClientJoin(queueEntry.Client);
                    break;
                case PacketId.PACKET_ID_PLAYER_JOINDATA:
                    PlayerJoinDataPacket joinDataPacket = new PlayerJoinDataPacket();
                    joinDataPacket.Read(queueEntry.Data);
                    HandleClientJoinData(queueEntry.Client, joinDataPacket);
                    break;
                case PacketId.PACKET_ID_PLAYER_QUIT:
                    HandleClientQuit(queueEntry.Client, QuitReason.Quit);
                    break;
                case PacketId.PACKET_ID_PLAYER_TIMEOUT:
                case PacketId.PACKET_ID_PLAYER_NO_SOCKET:
                    HandleClientQuit(queueEntry.Client, QuitReason.Timeout);
                    break;
            }
        }

        private void HandleClientJoin(Client client)
        {
            client.SendPacket(new ModNamePacket(0x06D, "deathmatch"));
        }

        private void HandleClientJoinData(Client client, PlayerJoinDataPacket joinDataPacket)
        {
            string osName =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Mac OS" :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
                RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ? "Free BSD" :
                "Unknown";
            client.SendPacket(new JoinCompletePacket($"Slipe Server 0.1.0 [{osName}]\0", "1.5.7-9.0.0"));

            client.Player.Name = joinDataPacket.Nickname;
            client.SetVersion(joinDataPacket.BitStreamVersion);
            client.FetchSerial();
        }

        private void HandleClientQuit(Client client, QuitReason reason)
        {
            var packet = PlayerPacketFactory.CreateQuitPacket(client.Player, reason);
            this.server.BroadcastPacket(packet);

            client.Player.Destroy();
        }
    }
}
