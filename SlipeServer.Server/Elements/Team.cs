using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A team element
/// A team represents a collection of players, along with some simple properties like a color, name and friendly fire setting.
/// </summary>
public class Team(string name, Color color) : Element(), IEnumerable
{
    public override ElementType ElementType => ElementType.Team;

    public string TeamName { get; set; } = name;
    public Color Color { get; set; } = color;
    public bool IsFriendlyFireEnabled { get; set; }

    private readonly Lock playersLock = new();
    private readonly List<Player> players = [];

    public IReadOnlyCollection<Player> Players => this.players.AsReadOnly();

    public void AddPlayer(Player player)
    {
        lock (this.playersLock)
        {
            if (this.players.Contains(player))
                return;

            this.players.Add(player);
        }

        player.Team = this;
        player.Disconnected += HandlePlayerDisconnect;
    }

    public void RemovePlayer(Player player)
    {
        lock (this.playersLock)
        {
            if (!this.players.Remove(player))
                return;
        }

        player.Team = null;
        player.Disconnected -= HandlePlayerDisconnect;
    }

    private void HandlePlayerDisconnect(Player sender, Events.PlayerQuitEventArgs e)
    {
        RemovePlayer(sender);
    }

    public new Team AssociateWith(IMtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public IEnumerator GetEnumerator() => this.players.GetEnumerator();
}
