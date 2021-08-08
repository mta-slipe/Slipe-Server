﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Repositories;

namespace SlipeServer.Server.Behaviour
{
    public class VoiceBehaviour
    {
        private readonly MtaServer server;
        private readonly ILogger logger;
        private readonly IElementRepository elementRepository;

        public VoiceBehaviour(MtaServer server, ILogger logger, IElementRepository elementRepository)
        {
            this.server = server;
            this.logger = logger;
            this.elementRepository = elementRepository;

            server.PlayerJoined += (player) =>
            {
                player.OnVoiceData += (sender, args) =>
                {
                    var packet = new VoiceDataPacket(player.Id, args.DataBuffer);
                    var otherPlayers = elementRepository.GetByType<Player>(ElementType.Player)
                        .Where(p => p.Client != player.Client).ToArray();

                    packet.SendTo(otherPlayers);
                };

                player.OnVoiceDataEnd += (sender, args) =>
                {
                    var packet = new VoiceEndPacket(player.Id);
                    var otherPlayers = elementRepository.GetByType<Player>(ElementType.Player)
                        .Where(p => p.Client != player.Client).ToArray();

                    packet.SendTo(otherPlayers);
                };
            };
        }

    }
}