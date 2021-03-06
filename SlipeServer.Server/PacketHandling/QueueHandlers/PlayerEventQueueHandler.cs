using SlipeServer.Packets.Definitions.Join;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Repositories;
using System;
using SlipeServer.Packets.Definitions.Player;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SlipeServer.Packets;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class PlayerEventQueueHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;
        public override IEnumerable<PacketId> SupportedPacketIds => new PacketId[] { PacketId.PACKET_ID_PLAYER_WASTED };

        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_PLAYER_WASTED] = typeof(PlayerWastedPacket),
        };

        public PlayerEventQueueHandler(
            ILogger logger,
            MtaServer server, 
            IElementRepository elementRepository, 
            int sleepInterval, 
            int workerCount
        ) : base(sleepInterval, workerCount)
        {
            this.logger = logger;
            this.server = server;
            this.elementRepository = elementRepository;
        }

        protected override void HandlePacket(Client client, Packet packet)
        {
            try
            {
                switch (packet)
                {
                    case PlayerWastedPacket wastedPacket:
                        HandlePlayerWasted(client, wastedPacket);
                        break;
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
        }

        private void HandlePlayerWasted(Client client, PlayerWastedPacket wastedPacket)
        {
            var damager = this.elementRepository.Get(wastedPacket.KillerId);
            client.Player.Kill(
                damager, (WeaponType)wastedPacket.WeaponType, (BodyPart)wastedPacket.BodyPart, 
                wastedPacket.AnimationGroup, wastedPacket.AnimationId
            );
        }
    }
}
