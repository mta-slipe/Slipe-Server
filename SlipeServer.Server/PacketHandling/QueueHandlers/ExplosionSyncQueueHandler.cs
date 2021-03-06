using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Explosions;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class ExplosionSyncQueueHandler : WorkerBasedQueueHandler
    {
        private readonly Configuration configuration;
        private readonly ILogger logger;
        private readonly IElementRepository elementRepository;

        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] { PacketId.PACKET_ID_EXPLOSION };

        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_EXPLOSION] = typeof(ExplosionPacket),
        };

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

        protected override void HandlePacket(Client client, Packet packet)
        {
            try
            { 
                switch (packet)
                {
                    case ExplosionPacket explosionPacket:
                        HandleExplosionPacket(client, explosionPacket);
                        break;
                }
            } catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
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
