using SlipeServer.Server.Elements.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerBindKeyArgs(Player player, string key, KeyState keyState) : EventArgs
{
    public Player Player { get; } = player;
    public string Key { get; } = key;
    public KeyState KeyState { get; } = keyState;
}
