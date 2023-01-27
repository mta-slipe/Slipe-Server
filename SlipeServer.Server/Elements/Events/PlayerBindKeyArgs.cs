using SlipeServer.Server.Elements.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerBindKeyArgs : EventArgs
{
    public Player Player { get; }
    public string Key { get; }
    public KeyState KeyState { get; }

    public PlayerBindKeyArgs(Player player, string key, KeyState keyState)
    {
        this.Player = player;
        this.Key = key;
        this.KeyState = keyState;
    }
}
