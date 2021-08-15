using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Satchels;
using SlipeServer.Packets.Definitions.Sync;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Constants;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.QueueHandlers.SyncMiddleware;
using SlipeServer.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class SatchelQueueHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        private readonly ISyncHandlerMiddleware<DetonateSatchelsPacket> detonateSatchelsMiddleware;
        private readonly ISyncHandlerMiddleware<DestroySatchelsPacket> destroySatchelsMiddleware;

        public override IEnumerable<PacketId> SupportedPacketIds => this.PacketTypes.Keys;

        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_DETONATE_SATCHELS] = typeof(DetonateSatchelsPacket),
            [PacketId.PACKET_ID_DESTROY_SATCHELS] = typeof(DestroySatchelsPacket),
        };

        public SatchelQueueHandler(
            ILogger logger,
            ISyncHandlerMiddleware<DetonateSatchelsPacket> detonateSatchelsMiddleware,
            ISyncHandlerMiddleware<DestroySatchelsPacket> destroySatchelsMiddleware,
            int sleepInterval,
            int workerCount
        ) : base(sleepInterval, workerCount)
        {
            this.logger = logger;
            this.detonateSatchelsMiddleware = detonateSatchelsMiddleware;
            this.destroySatchelsMiddleware = destroySatchelsMiddleware;
        }

        protected override void HandlePacket(Client client, Packet packet)
        {
            try
            {
                switch (packet)
                {
                    case DetonateSatchelsPacket detonateSatchelsPacket:
                        HandleDetonateSatchelsPacket(client, detonateSatchelsPacket);
                        break;
                    case DestroySatchelsPacket destroySatchelsPacket:
                        HandleDestroySatchelsPacket(client, destroySatchelsPacket);
                        break;
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
        }

        private void HandleDetonateSatchelsPacket(Client client, DetonateSatchelsPacket packet)
        {
            packet.ElementId = client.Player.Id;
            packet.Latency = (ushort)client.Ping;

            var otherPlayers = this.detonateSatchelsMiddleware.GetPlayersToSyncTo(client.Player, packet);
            packet.SendTo(otherPlayers);
        }

        private void HandleDestroySatchelsPacket(Client client, DestroySatchelsPacket packet)
        {
            packet.ElementId = client.Player.Id;

            var otherPlayers = this.destroySatchelsMiddleware.GetPlayersToSyncTo(client.Player, packet);
            packet.SendTo(otherPlayers);
        }
    }
}
