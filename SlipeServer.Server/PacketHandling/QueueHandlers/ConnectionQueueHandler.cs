using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Runtime.InteropServices;
using SlipeServer.Server.Extensions;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SlipeServer.Packets;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class ConnectionQueueHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;
        private readonly ushort bitStreamVersion;

        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] { 
            PacketId.PACKET_ID_PLAYER_JOIN,
            PacketId.PACKET_ID_PLAYER_JOINDATA,
            PacketId.PACKET_ID_PLAYER_QUIT,
            PacketId.PACKET_ID_PLAYER_TIMEOUT,
            PacketId.PACKET_ID_PLAYER_NO_SOCKET
        };

        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_PLAYER_JOIN] = typeof(JoinedGamePacket),
            [PacketId.PACKET_ID_PLAYER_JOINDATA] = typeof(PlayerJoinDataPacket),
            [PacketId.PACKET_ID_PLAYER_QUIT] = typeof(PlayerQuitPacket),
            [PacketId.PACKET_ID_PLAYER_TIMEOUT] = typeof(PlayerTimeoutPacket),
            [PacketId.PACKET_ID_PLAYER_NO_SOCKET] = typeof(NoSocketPacket),
        };

        public ConnectionQueueHandler(
            ILogger logger,
            MtaServer server, 
            IElementRepository elementRepository, 
            int sleepInterval, 
            int workerCount,
            Configuration configuration
        ) : base(sleepInterval, workerCount)
        {
            this.logger = logger;
            this.server = server;
            this.elementRepository = elementRepository;

            this.bitStreamVersion = configuration.BitStreamVersion;
        }

        protected override void HandlePacket(Client client, Packet packet)
        {
            try
            {
                switch (packet)
                {
                    case JoinedGamePacket:
                        HandleClientJoin(client);
                        break;
                    case PlayerJoinDataPacket joinDataPacket:
                        HandleClientJoinData(client, joinDataPacket);
                        break;
                    case PlayerQuitPacket:
                        HandleClientQuit(client, QuitReason.Quit);
                        break;
                    case PlayerTimeoutPacket:
                    case NoSocketPacket:
                        HandleClientQuit(client, QuitReason.Timeout);
                        break;
                }
            } catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
        }

        private void HandleClientJoin(Client client)
        {
            client.SendPacket(new ModNamePacket(this.bitStreamVersion, "deathmatch"));
        }

        private void HandleClientJoinData(Client client, PlayerJoinDataPacket joinDataPacket)
        {
            string osName =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Mac OS" :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
                RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) ? "Free BSD" :
                "Unknown";
            client.SendPacket(new JoinCompletePacket($"Slipe Server 0.1.0 [{osName}]", "1.5.8-9.0.0"));

            client.Player.RunAsSync(() =>
            {
                client.Player.Name = joinDataPacket.Nickname;
            });
            client.SetVersion(joinDataPacket.BitStreamVersion);
            client.FetchSerial();
        }

        private void HandleClientQuit(Client client, QuitReason reason)
        {
            var packet = PlayerPacketFactory.CreateQuitPacket(client.Player, reason);
            packet.SendTo(this.elementRepository.GetByType<Player>(ElementType.Player).Except(new Player[] { client.Player }));

            client.Player.Destroy();
        }
    }
}
