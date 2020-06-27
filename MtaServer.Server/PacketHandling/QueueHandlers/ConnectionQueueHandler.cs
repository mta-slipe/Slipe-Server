using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MtaServer.Server.PacketHandling.QueueHandlers
{
    public class ConnectionQueueHandler : WorkerBasedQueueHandler
    {
        public ConnectionQueueHandler(MtaServer server, int sleepInterval, int workerCount): base(server, sleepInterval, workerCount) { }

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
                case PacketId.PACKET_ID_PLAYER_TIMEOUT:
                    HandleClientQuit(queueEntry.Client);
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

        private void HandleClientQuit(Client client)
        {
            server.ElementRepository.Remove(client.Player);
        }
    }
}
