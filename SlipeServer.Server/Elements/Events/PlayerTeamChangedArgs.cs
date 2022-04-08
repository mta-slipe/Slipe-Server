using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerTeamChangedArgs : EventArgs
{
    public Player SourcePlayer { get; set; }
    public Team? PreviousTeam { get; set; }
    public Team? NewTeam { get; set; }

    public PlayerTeamChangedArgs(Player sourcePlayer, Team? newTeam, Team? previousTeam = null)
    {
        this.SourcePlayer = sourcePlayer;
        this.NewTeam = newTeam;
        this.PreviousTeam = previousTeam;
    }
}
