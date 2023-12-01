using System.Linq;
using Microsoft.Extensions.Logging;
using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.ElementCollections;

namespace SlipeServer.Server.Behaviour;

/// <summary>
/// Behaviour responsible for sending create entity packets for all elements in the collection on player join 
/// </summary>
public class PlayerJoinElementBehaviour
{
    private readonly IElementCollection elementCollection;
    private readonly ILogger logger;

    public PlayerJoinElementBehaviour(IElementCollection elementCollection, MtaServer server, ILogger logger)
    {
        this.elementCollection = elementCollection;
        this.logger = logger;
        server.PlayerJoined += OnPlayerJoin;
    }

    private void OnPlayerJoin(Player player)
    {
        player.Disconnected += OnPlayerDisconnect;

        this.logger.LogInformation("{Name} ({Version}) ({Serial}) has joined the server!", player.Name, player.Client.Version, player.Client.Serial);
    }

    private void OnPlayerDisconnect(Player player, Elements.Events.PlayerQuitEventArgs e)
    {
        this.logger.LogInformation("{Name} ({Version}) ({Serial}) has left the server!", player!.Name, player.Client.Version, player.Client.Serial);

        var packet = new PlayerQuitPacket(player.Id, (byte)e.Reason);

        var otherPlayers = this.elementCollection
            .GetByType<Player>(ElementType.Player)
            .Where(p => p != player);
        packet.SendTo(otherPlayers);
    }
}
