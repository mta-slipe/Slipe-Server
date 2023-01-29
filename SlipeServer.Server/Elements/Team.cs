using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A team element
/// A team represents a collection of players, along with some simple properties like a color, name and friendly fire setting.
/// </summary>
public class Team : Element, IEnumerable
{
    public override ElementType ElementType => ElementType.Team;

    public string TeamName { get; set; }
    public Color Color { get; set; }
    public bool IsFriendlyFireEnabled { get; set; }
    public List<Player> Players { get; set; }

    public Team(string name, Color color) : base()
    {
        this.TeamName = name;
        this.Color = color;

        this.Players = new List<Player>();
    }

    public new Team AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public IEnumerator GetEnumerator() => this.Players.GetEnumerator();
}
