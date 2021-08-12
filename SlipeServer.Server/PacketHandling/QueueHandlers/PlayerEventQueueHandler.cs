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
            [PacketId.PACKET_ID_PLAYER_DIAGNOSTIC] = typeof(PlayerDiagnosticPacket),
            [PacketId.PACKET_ID_PLAYER_ACINFO] = typeof(PlayerACInfoPacket),
            [PacketId.PACKET_ID_PLAYER_MODINFO] = typeof(PlayerModInfoPacket),
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
                    case PlayerDiagnosticPacket diagnosticPacket:
                        HandleDiagnosticPacket(client, diagnosticPacket);
                        break;
                    case PlayerACInfoPacket acInfoPacket:
                        HandleAcInfoPacket(client, acInfoPacket);
                        break;
                    case PlayerModInfoPacket modInfoPacket:
                        HandleModInfoPacket(client, modInfoPacket);
                        break;
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
        }

        private void HandleDiagnosticPacket(Client client, PlayerDiagnosticPacket diagnosticPacket)
        {
            if(diagnosticPacket.Level == 236)
            {
                client.Player.TriggerPlayerACInfo(diagnosticPacket.DetectedAC, diagnosticPacket.D3d9Size, diagnosticPacket.D3d9Md5, diagnosticPacket.D3d9Sha256);
            }
            else
            {
                client.Player.TriggerPlayerDiagnosticInfo(diagnosticPacket.Level, diagnosticPacket.Message);
            }
        }

        private void HandleModInfoPacket(Client client, PlayerModInfoPacket modInfoPacket)
        {

        }
        private void HandleAcInfoPacket(Client client, PlayerACInfoPacket acInfoPacket)
        {
            client.Player.TriggerPlayerACInfo(acInfoPacket.DetectedACList, acInfoPacket.D3d9Size, acInfoPacket.D3d9MD5, acInfoPacket.D3d9SHA256);
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
                            Tag = screenshotPacket.Tag,
                        };
                    }
                }
                else
                {

                    client.Player.PendingScreenshots[screenshotPacket.ScreenshotId] = new PlayerPendingScreenshot
                    {
                        ErrorMessage = screenshotPacket.Error,
                        Tag = screenshotPacket.Tag,
                    };
                    client.Player.ScreenshotEnd(screenshotPacket.ScreenshotId);
                    return;
                }
            }
            var pendingScreenshot = client.Player.PendingScreenshots[screenshotPacket.ScreenshotId];
            if (pendingScreenshot.Stream != null)
            {
                pendingScreenshot.Stream.Write(screenshotPacket.Buffer);
                if (pendingScreenshot.TotalParts == screenshotPacket.PartNumber + 1)
                {
                    pendingScreenshot.Stream.Seek(0, SeekOrigin.Begin);
                    client.Player.ScreenshotEnd(screenshotPacket.ScreenshotId);
                }
            }
        }
    }
}
