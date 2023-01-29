using System;

namespace SlipeServer.Server.Elements.Events;

public sealed class PlayerCommandEventArgs : EventArgs
{
    public Player Source { get; }
    public string Command { get; }
    public string[] Arguments { get; }

    public PlayerCommandEventArgs(Player source, string command, string[] arguments)
    {
        this.Source = source;
        this.Command = command;
        this.Arguments = arguments;
    }
}
