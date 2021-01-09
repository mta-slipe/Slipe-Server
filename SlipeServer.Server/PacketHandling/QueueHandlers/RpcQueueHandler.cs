using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Enums;
using SlipeServer.Packets.Rpc;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class RpcQueueHandler : WorkerBasedQueueHandler
    {
        private readonly IElementRepository elementRepository;
        private readonly Configuration configuration;
        private readonly ILogger logger;
        private readonly MtaServer server;
        private readonly RootElement root;
        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] { PacketId.PACKET_ID_RPC };

        public RpcQueueHandler(
            ILogger logger, 
            MtaServer server, 
            RootElement root, 
            IElementRepository elementRepository, 
            Configuration configuration, 
            int sleepInterval, 
            int workerCount
        ) : base(sleepInterval, workerCount)
        {
            this.logger = logger;
            this.server = server;
            this.root = root;
            this.elementRepository = elementRepository;
            this.configuration = configuration;
        }

        protected override void HandlePacket(PacketQueueEntry queueEntry)
        {
            try
            {
                switch (queueEntry.PacketId)
                {
                    case PacketId.PACKET_ID_RPC:
                        RpcPacket packet = new RpcPacket();
                        packet.Read(queueEntry.Data);
                        HandleRpc(queueEntry.Client, packet);
                        break;
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({queueEntry.PacketId}) failed.\n{e.Message}");
            }
        }

        private void HandleRpc(Client client, RpcPacket packet)
        {
            switch (packet.FunctionId)
            {
                case RpcFunctions.PLAYER_INGAME_NOTICE:
                    var players = this.elementRepository.GetByType<Player>(ElementType.Player);

                    client.SendPacket(new JoinedGamePacket(
                        client.Player.Id,
                        players.Count ()+ 1, 
                        this.root.Id,
                        configuration.HttpUrl != null ? HttpDownloadType.HTTP_DOWNLOAD_ENABLED_URL : HttpDownloadType.HTTP_DOWNLOAD_ENABLED_PORT, 
                        configuration.HttpPort, 
                        configuration.HttpUrl ?? "", 
                        configuration.HttpConnectionsPerClient, 
                        1
                    ));

                    var otherPlayers = players
                        .Except(new Player[] { client.Player })
                        .ToArray();

                    var existingPlayersListPacket = PlayerPacketFactory.CreatePlayerListPacket(otherPlayers, true);
                    client.SendPacket(existingPlayersListPacket);

                    var newPlayerListPacket = PlayerPacketFactory.CreatePlayerListPacket(new Player[] { client.Player }, false);
                    newPlayerListPacket.SendTo(otherPlayers);

                    this.server.HandlePlayerJoin(client.Player);

                    break;
                default:
                    this.logger.LogWarning($"Received RPC of type {packet.FunctionId}");
                    break;
            }
        }
    }
}
