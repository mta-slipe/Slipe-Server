using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Enums;
using MtaServer.Packets.Rpc;
using MtaServer.Server.Elements;
using MtaServer.Server.Extensions;
using MtaServer.Server.PacketHandling.Factories;
using MtaServer.Server.Repositories;
using System;
using System.Diagnostics;
using System.Linq;

namespace MtaServer.Server.PacketHandling.QueueHandlers
{
    public class RpcQueueHandler : WorkerBasedQueueHandler
    {
        private readonly IElementRepository elementRepository;
        private readonly Configuration configuration;
        private readonly MtaServer server;
        private readonly RootElement root;

        public RpcQueueHandler(MtaServer server, RootElement root, IElementRepository elementRepository, Configuration configuration, int sleepInterval, int workerCount)
            : base(sleepInterval, workerCount)
        {
            this.server = server;
            this.root = root;
            this.elementRepository = elementRepository;
            this.configuration = configuration;
        }

        protected override void HandlePacket(PacketQueueEntry queueEntry)
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

        private void HandleRpc(Client client, RpcPacket packet)
        {
            Debug.WriteLine($"Received RPC of type {packet.FunctionId}");
            switch (packet.FunctionId)
            {
                case RpcFunctions.PLAYER_INGAME_NOTICE:
                    client.SendPacket(new JoinedGamePacket(
                        client.Player.Id, 
                        this.elementRepository.Count + 1, 
                        this.root.Id,
                        configuration.HttpUrl != null ? HttpDownloadType.HTTP_DOWNLOAD_ENABLED_URL : HttpDownloadType.HTTP_DOWNLOAD_ENABLED_PORT, 
                        configuration.HttpPort, 
                        configuration.HttpUrl ?? "", 
                        configuration.HttpConnectionsPerClient, 
                        1
                    ));

                    var otherPlayers = this.elementRepository
                        .GetByType<Player>(ElementType.Player)
                        .Except(new Player[] { client.Player })
                        .ToArray();

                    var existingPlayersListPacket = PlayerPacketFactory.CreatePlayerListPacket(otherPlayers, true);
                    client.SendPacket(existingPlayersListPacket);

                    var newPlayerListPacket = PlayerPacketFactory.CreatePlayerListPacket(new Player[] { client.Player }, false);
                    newPlayerListPacket.SendTo(otherPlayers);

                    this.server.HandlePlayerJoin(client.Player);

                    break;
            }
        }
    }
}
