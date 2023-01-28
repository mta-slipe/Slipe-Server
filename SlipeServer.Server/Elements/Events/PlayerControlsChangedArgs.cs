using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerControlsChangedArgs : EventArgs
{
    public Player Player { get; }
    public string Control { get; }
    public bool NewState { get; }

    public PlayerControlsChangedArgs(Player player, string control, bool newState)
    {
        this.Player = player;
        this.Control = control;
        this.NewState = newState;
    }
}
