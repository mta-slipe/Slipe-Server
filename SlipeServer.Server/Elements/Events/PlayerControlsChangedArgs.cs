using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerControlsChangedArgs : EventArgs
{
    public Player Player { get; }
    public string[] Controls { get; }
    public bool NewState { get; }

    public PlayerControlsChangedArgs(Player player, string control, bool newState)
    {
        this.Player = player;
        this.Controls = [control];
        this.NewState = newState;
    }

    public PlayerControlsChangedArgs(Player player, string[] controls, bool newState)
    {
        this.Player = player;
        this.Controls = controls;
        this.NewState = newState;
    }
}
