using System.Linq;
using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.Behaviour;

public class VoiceBehaviour
{
    public VoiceBehaviour(MtaServer server, IElementCollection elementCollection)
    {
        server.PlayerJoined += (player) =>
        {
            player.VoiceDataReceived += (sender, args) =>
            {
                var packet = new VoiceDataPacket(player.Id, args.DataBuffer);
                var otherPlayers = elementCollection.GetByType<Player>(ElementType.Player)
                    .Where(p => p.Client != player.Client).ToArray();

                packet.SendTo(otherPlayers);
            };

            player.VoiceDataEnded += (sender, args) =>
            {
                var packet = new VoiceEndPacket(player.Id);
                var otherPlayers = elementCollection.GetByType<Player>(ElementType.Player)
                    .Where(p => p.Client != player.Client).ToArray();

                packet.SendTo(otherPlayers);
            };
        };
    }

}
