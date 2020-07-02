using MtaServer.Packets.Definitions.Join;
using MtaServer.Packets.Enums;
using MtaServer.Packets.Rpc;
using MtaServer.Server.Elements;
using MtaServer.Server.PacketHandling.Factories;
using MtaServer.Server.Repositories;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MtaServer.Server.PacketHandling.QueueHandlers
{
    public class RpcQueueHandler : WorkerBasedQueueHandler
    {
        private readonly IElementRepository elementRepository;
        private readonly RootElement root;

        public RpcQueueHandler(RootElement root, IElementRepository elementRepository, int sleepInterval, int workerCount)
            : base(sleepInterval, workerCount) 
        {
            this.elementRepository = elementRepository;
            this.root = root;
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
                        HttpDownloadType.HTTP_DOWNLOAD_ENABLED_PORT, 
                        80, 
                        "", 
                        5, 
                        1
                    ));

                    var existingPlayersListPacket = PlayerPacketFactory.CreatePlayerListPacket(
                        this.elementRepository.GetByType<Player>(ElementType.Player).ToArray(), 
                        true
                    );
                    client.SendPacket(existingPlayersListPacket);

                    var newPlayerListPacket = PlayerPacketFactory.CreatePlayerListPacket(new Player[] { client.Player }, false);
                    foreach (var player in this.elementRepository.GetByType<Player>(ElementType.Player))
                    {
                        player.Client.SendPacket(newPlayerListPacket);
                    }

                    this.elementRepository.Add(client.Player);
                    client.Player.HandleJoin();

                    break;
            }
        }
    }
}
