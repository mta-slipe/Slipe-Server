using System.Linq;
using SlipeServer.Packets.Definitions.Voice;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for relaying voice chat information
/// </summary>
public class VoiceBehaviour
{
    public VoiceBehaviour(IMtaServer server, IElementCollection elementCollection)
    {
        server.PlayerJoined += (player) =>
        {
            player.VoiceDataReceived += (sender, args) =>
            {
                var packet = new VoiceDataPacket(player.Id, args.DataBuffer);
                var otherPlayers = elementCollection.GetByType<Player>(ElementType.Player)
                    .Where(p => p.Client != player.Client)
                    .Where(p => IsInBroadcastTarget(p, player.VoiceBroadcastTo))
                    .Where(p => !IsIgnoredBy(p, player))
                    .ToArray();

                packet.SendTo(otherPlayers);
            };

            player.VoiceDataEnded += (sender, args) =>
            {
                var packet = new VoiceEndPacket(player.Id);
                var otherPlayers = elementCollection.GetByType<Player>(ElementType.Player)
                    .Where(p => p.Client != player.Client)
                    .Where(p => IsInBroadcastTarget(p, player.VoiceBroadcastTo))
                    .Where(p => !IsIgnoredBy(p, player))
                    .ToArray();

                packet.SendTo(otherPlayers);
            };
        };
    }

    private static bool IsInBroadcastTarget(Player listener, Element? broadcastTo)
    {
        if (broadcastTo == null)
            return true;

        if (broadcastTo is Player targetPlayer)
            return listener == targetPlayer;

        if (broadcastTo is Team targetTeam)
            return listener.Team == targetTeam;

        return false;
    }

    private static bool IsIgnoredBy(Player listener, Player speaker)
    {
        if (listener.VoiceIgnoreFrom == null)
            return false;

        if (listener.VoiceIgnoreFrom is Player ignoredPlayer)
            return speaker == ignoredPlayer;

        if (listener.VoiceIgnoreFrom is Team ignoredTeam)
            return speaker.Team == ignoredTeam;

        return false;
    }
}
