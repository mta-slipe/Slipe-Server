using System;

namespace SlipeServer.Server.Elements.Events;

public class PlayerCommandEventArgs : EventArgs
{
    public Player Source { get; }
    public string Command { get; set; }
    public string[] Arguments { get; set; }

    public PlayerCommandEventArgs(Player source, string command, string[] arguments)
    {
        this.Source = source;
        this.Command = command;
        this.Arguments = arguments;
    }
}
