using SlipeServer.Server.Elements.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerBindExecutedEventArgs : EventArgs
{
    public Player Player { get; }
    public BindType BindType { get; }
    public string Key { get; }
    public KeyState KeyState { get; }

    public PlayerBindExecutedEventArgs(Player player, BindType bindType, KeyState keyState, string key)
    {
        this.Player = player;
        this.BindType = bindType;
        this.KeyState = keyState;
        this.Key = key;
    }
}
