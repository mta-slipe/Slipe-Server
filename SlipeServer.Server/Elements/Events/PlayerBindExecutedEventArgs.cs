using SlipeServer.Server.Elements.Enums;
using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerBindExecutedEventArgs(Player player, BindType bindType, KeyState keyState, string key) : EventArgs
{
    public Player Player { get; } = player;
    public BindType BindType { get; } = bindType;
    public string Key { get; } = key;
    public KeyState KeyState { get; } = keyState;
}
