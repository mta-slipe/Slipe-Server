using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerTeamChangedArgs(Player sourcePlayer, Team? newTeam, Team? previousTeam = null) : EventArgs
{
    public Player SourcePlayer { get; } = sourcePlayer;
    public Team? PreviousTeam { get; } = previousTeam;
    public Team? NewTeam { get; } = newTeam;
}
