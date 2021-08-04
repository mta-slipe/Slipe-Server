using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;

namespace SlipeServer.Server.Behaviour
{
    public class VoiceBehaviour
    {
        private readonly MtaServer server;
        private readonly ILogger logger;

        public VoiceBehaviour(MtaServer server, ILogger logger)
        {
            this.server = server;
            this.logger = logger;

            server.PlayerJoined += (player) =>
            {
                player.OnVoiceData += (sender, args) =>
                {
                    var packet = new VoiceDataPacket(player.Id, args.DataBuffer);
                    server.BroadcastPacket(packet);
                };

                player.OnVoiceDataEnd += (sender, args) =>
                {
                    var packet = new VoiceEndPacket(player.Id);
                    server.BroadcastPacket(packet);
                };
            };
            //TODO: Add onPlayerVoiceStart & onPlayerVoiceStop events
        }

    }
}
