using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.Behaviour;

public class VoiceBehaviour
{
    public VoiceBehaviour(MtaServer server, IElementCollection elementRepository)
    {
        server.PlayerJoined += (player) =>
        {
            player.VoiceDataReceived += (sender, args) =>
            {
                var packet = new VoiceDataPacket(player.Id, args.DataBuffer);
                var otherPlayers = elementRepository.GetByType<Player>(ElementType.Player)
                    .Where(p => p.Client != player.Client).ToArray();

                packet.SendTo(otherPlayers);
            };

            player.VoiceDataEnded += (sender, args) =>
            {
                var packet = new VoiceEndPacket(player.Id);
                var otherPlayers = elementRepository.GetByType<Player>(ElementType.Player)
                    .Where(p => p.Client != player.Client).ToArray();

                packet.SendTo(otherPlayers);
            };
        };
    }

}
