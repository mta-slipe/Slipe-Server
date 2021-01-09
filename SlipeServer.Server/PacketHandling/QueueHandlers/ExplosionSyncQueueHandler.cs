using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Repositories;


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class ExplosionSyncQueueHandler : WorkerBasedQueueHandler
    {
        private readonly Configuration configuration;
        private readonly ILogger logger;
        private readonly IElementRepository elementRepository;

        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] { PacketId.PACKET_ID_EXPLOSION };

        public ExplosionSyncQueueHandler(
            Configuration configuration,
            ILogger logger,
            IElementRepository elementRepository,
            int sleepInterval, 
            int workerCount
        ) : base(sleepInterval, workerCount)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.elementRepository = elementRepository;
        }

        protected override void HandlePacket(PacketQueueEntry queueEntry)
        {
            try
            { 
                switch (queueEntry.PacketId)
                {
                    case PacketId.PACKET_ID_EXPLOSION:
                        ExplosionPacket explosioPacket = new ExplosionPacket();
                        explosioPacket.Read(queueEntry.Data);
                        HandleExplosionPacket(queueEntry.Client, explosioPacket);
                        break;
                }
            } catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({queueEntry.PacketId}) failed.\n{e.Message}");
            }
        }

        private void HandleExplosionPacket(Client client, ExplosionPacket packet)
        {
            var player = client.Player;
            packet.PlayerSource = player.Id;

            if (packet.OriginId != null)
            {
                var explosionorigin = this.elementRepository.Get(packet.OriginId.Value);
                if (explosionorigin != null)
                {
                    if (explosionorigin is Vehicle vehicle)
                    {
                        vehicle.BlowUp();
                    }
                }
            }

            var nearbyPlayers = this.elementRepository.GetWithinRange<Player>(
                packet.Position, 
                this.configuration.ExplosionSyncDistance,
                ElementType.Player
            );

            packet.SendTo(nearbyPlayers);
        }
    }
}
