using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Enums;
using MtaServer.Server.Elements;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MtaServer.Server.PacketHandling.QueueHandlers
{
    public class ConnectionQueueHandler : BaseQueueHandler
    {
        private readonly MtaServer server;
        private readonly int sleepInterval;

        public ConnectionQueueHandler(MtaServer server, int sleepInterval, int workerCount): base()
        {
            this.server = server;
            this.sleepInterval = sleepInterval;

            for (int i = 0; i < workerCount; i++)
            {
                Task.Run(HandlePackets);
            }
        }

        public async void HandlePackets()
        {
            while (true)
            {
                while(this.packetQueue.TryDequeue(out PacketQueueEntry queueEntry))
                {
                    switch (queueEntry.PacketId)
                    {
                        case PacketId.PACKET_ID_PLAYER_JOIN:
                            HandlePlayerJoin(queueEntry.Client);
                            break;
                        case PacketId.PACKET_ID_PLAYER_JOINDATA:
                            PlayerJoinDataPacket joinDataPacket = new PlayerJoinDataPacket();
                            joinDataPacket.Read(queueEntry.Data);
                            HandlePlayerJoinData(queueEntry.Client, joinDataPacket);
                            break;
                        case PacketId.PACKET_ID_PLAYER_QUIT:
                        case PacketId.PACKET_ID_PLAYER_TIMEOUT:
                            HandlePlayerQuit(queueEntry.Client);
                            break;
                    }
                }
                await Task.Delay(this.sleepInterval);
            }
        }

        private void HandlePlayerJoin(Client client)
        {
            client.SendPacket(new ModNamePacket(0x06D, "deathmatch"));
        }

        private void HandlePlayerJoinData(Client client, PlayerJoinDataPacket joinDataPacket)
        {
            string osName =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Mac OS" :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
                RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ? "Free BSD" :
                "Unknown";
            client.SendPacket(new JoinCompletePacket($"Slipe Server 0.1.0 [{osName}]\0", "1.5.7-9.0.0"));

            client.Name = joinDataPacket.Nickname;
            client.SetVersion(joinDataPacket.BitStreamVersion);
            client.FetchSerial();
        }

        private void HandlePlayerQuit(Client client)
        {
            server.ElementRepository.Remove(client);
        }
    }
}
