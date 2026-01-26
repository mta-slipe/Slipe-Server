using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerCommandEventArgs(Player source, string command, string[] arguments) : EventArgs
{
    public Player Source { get; } = source;
    public string Command { get; } = command;
    public string[] Arguments { get; } = arguments;
}
