using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets;
using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Packets.Enums;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.PacketHandling.QueueHandlers
{
    public class VoiceHandler : WorkerBasedQueueHandler
    {
        private readonly ILogger logger;
        private readonly MtaServer server;
        private readonly Configuration configuration;
        private readonly IElementRepository elementRepository;
        private readonly RootElement root;
        public override IEnumerable<PacketId> SupportedPacketIds { get; } = new PacketId[]
        {
            PacketId.PACKET_ID_VOICE_DATA,
            PacketId.PACKET_ID_VOICE_END
        };
        protected override Dictionary<PacketId, Type> PacketTypes { get; } = new Dictionary<PacketId, Type>()
        {
            [PacketId.PACKET_ID_VOICE_DATA] = typeof(VoiceDataPacket),
            [PacketId.PACKET_ID_VOICE_END] = typeof(VoiceEndPacket)
        };

        public VoiceHandler(ILogger logger,
            int sleepInterval,
            int workerCount,
            MtaServer server,
            IElementRepository elementRepository,
            Configuration configuration,
            RootElement root) : base(sleepInterval, workerCount)
        {
            this.logger = logger;
            this.server = server;
            this.elementRepository = elementRepository;
            this.configuration = configuration;
            this.root = root;
        }
        
        protected override void HandlePacket(Client client, Packet packet)
        {
            logger.LogInformation("VoiceHandler Received a packet!");
            try
            {
                switch (packet)
                {
                    case VoiceDataPacket voiceDataPacket:
                        this.HandleVoiceData(client, voiceDataPacket);
                        break;
                    case VoiceEndPacket voiceEndPacket:
                        this.HandleVoidEnd(client, voiceEndPacket);
                        break;
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"Handling packet ({packet.PacketId}) failed.\n{e.Message}");
            }
        }

        private void HandleVoidEnd(Client client, VoiceEndPacket packet)
        {
            client.Player.VoiceDataEnd();
        }

        private void HandleVoiceData(Client client, VoiceDataPacket packet)
        {
            client.Player.VoiceDataStart(packet.Buffer);
        }
    }
}