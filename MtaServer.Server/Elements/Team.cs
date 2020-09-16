using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MtaServer.Server.Elements
{
    public class Team : Element
    {
        public override ElementType ElementType => ElementType.Team;

        public string TeamName { get; set; }
        public Color Color { get; set; }
        public bool IsFriendlyFireEnabled { get; set; }
        public IEnumerable<Player> Players { get; set; }

        public Team(string name, Color color): base()
        {
            this.TeamName = name;
            this.Color = color;

            this.Players = new List<Player>();
        }

        public new Team AssociateWith(MtaServer server)
        {
            return server.AssociateElement(this);
        }
    }
}
