using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using MTAServerWrapper.Packets.Outgoing.Connection;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.PacketHandling.Factories;
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
        var elements = this.elementCollection.GetAll();
        var packet = AddEntityPacketFactory.CreateAddEntityPacket(elements);
        player.Client.SendPacket(packet);

        player.Disconnected += OnPlayerDisconnect;

        this.logger.LogInformation($"{player.Name} ({player.Client.Version}) ({player.Client.Serial}) has joined the server!");
    }

    private void OnPlayerDisconnect(object? sender, Elements.Events.PlayerQuitEventArgs e)
    {
        var player = sender as Player;
        this.logger.LogInformation($"{player!.Name} ({player.Client.Version}) ({player.Client.Serial}) has left the server!");

        var packet = new PlayerQuitPacket(player.Id, (byte)e.Reason);

        var otherPlayers = this.elementCollection
            .GetByType<Player>(ElementType.Player)
            .Where(p => p != player);
        packet.SendTo(otherPlayers);
    }
}
