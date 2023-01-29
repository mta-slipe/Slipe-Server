using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerTeamChangedArgs : EventArgs
{
    public Player SourcePlayer { get; }
    public Team? PreviousTeam { get; }
    public Team? NewTeam { get; }

    public PlayerTeamChangedArgs(Player sourcePlayer, Team? newTeam, Team? previousTeam = null)
    {
        this.SourcePlayer = sourcePlayer;
        this.NewTeam = newTeam;
        this.PreviousTeam = previousTeam;
    }
}
