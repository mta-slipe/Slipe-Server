using SlipeServer.Server.Elements.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerBindExecutedEventArgs : EventArgs
{
    public Player Player { get; set; }
    public BindType BindType { get; set; }
    public string Key { get; set; }
    public KeyState KeyState { get; set; }

    public PlayerBindExecutedEventArgs(Player player, BindType bindType, KeyState keyState, string key)
    {
        this.Player = player;
        this.BindType = bindType;
        this.KeyState = keyState;
        this.Key = key;
    }
}
