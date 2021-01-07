using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.PacketHandling.Factories;
using SlipeServer.Server.Repositories;
using MTAServerWrapper.Packets.Outgoing.Connection;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SlipeServer.Server.Extensions;
using System.Linq;
using SlipeServer.Packets.Definitions.Player;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class PlayerEventQueueHandler : WorkerBasedQueueHandler
    {
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;

        public PlayerEventQueueHandler(MtaServer server, IElementRepository elementRepository, int sleepInterval, int workerCount)
            : base(sleepInterval, workerCount)
        {
            this.server = server;
            this.elementRepository = elementRepository;
        }

        protected override void HandlePacket(PacketQueueEntry queueEntry)
        {
            switch (queueEntry.PacketId)
            {
                case PacketId.PACKET_ID_PLAYER_WASTED:
                    PlayerWastedPacket wastedPacket = new PlayerWastedPacket();
                    wastedPacket.Read(queueEntry.Data);
                    HandlePlayerWasted(queueEntry.Client, wastedPacket);
                    break;
            }
        }

        private void HandlePlayerWasted(Client client, PlayerWastedPacket wastedPacket)
        {
            var damager = this.elementRepository.Get(wastedPacket.KillerId);
            client.Player.Kill(damager, (WeaponType)wastedPacket.WeaponType, (BodyPart)wastedPacket.BodyPart);
        }
    }
}
