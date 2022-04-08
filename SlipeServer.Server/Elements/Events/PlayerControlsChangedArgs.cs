using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerControlsChangedArgs : EventArgs
{
    public Player Player { get; set; }
    public string Control { get; set; }
    public bool NewState { get; set; }

    public PlayerControlsChangedArgs(Player player, string control, bool newState)
    {
        this.Player = player;
        this.Control = control;
        this.NewState = newState;
    }
}
