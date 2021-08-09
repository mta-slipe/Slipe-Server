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
using System.IO;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class PlayerEventQueueHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        private readonly MtaServer server;
        private readonly IElementRepository elementRepository;
        public override IEnumerable<PacketId> SupportedPacketIds => PacketTypes.Keys;

        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_PLAYER_WASTED] = typeof(PlayerWastedPacket),
            [PacketId.PACKET_ID_PLAYER_SCREENSHOT] = typeof(PlayerScreenshotPacket),
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
                    case PlayerScreenshotPacket screenshotPacket:
                        HandlePlayerScreenshot(client, screenshotPacket);
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

        private void HandlePlayerScreenshot(Client client, PlayerScreenshotPacket screenshotPacket)
        {
            if (screenshotPacket.PartNumber == 0)
            {
                if (screenshotPacket.Status == ScreenshotStatus.Success)
                {
                    if (!(client.Player.PendingScreenshots.ContainsKey(screenshotPacket.ScreenshotId)))
                    {
                        client.Player.PendingScreenshots[screenshotPacket.ScreenshotId] = new PlayerPendingScreenshot
                        {
                            Stream = new MemoryStream((int)screenshotPacket.TotalBytes),
                            TotalParts = screenshotPacket.TotalParts,
                        };
                    }
                }
            }
            var pendingScreenshot = client.Player.PendingScreenshots[screenshotPacket.ScreenshotId];
            pendingScreenshot.Stream.Write(screenshotPacket.Buffer);
            if (pendingScreenshot.TotalParts == screenshotPacket.PartNumber + 1)
            {
                pendingScreenshot.Stream.Seek(0, SeekOrigin.Begin);
                client.Player.ScreenshotEnd(screenshotPacket.ScreenshotId);
            }
        }
    }
}
