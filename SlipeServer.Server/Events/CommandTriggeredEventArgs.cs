using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Server.Events;

public sealed class CommandTriggeredEventArgs : EventArgs
{
    public Player Player { get; }
    public string[] Arguments { get; }

    public CommandTriggeredEventArgs(Player player, string[] arguments)
    {
        this.Player = player;
        this.Arguments = arguments;
    }
}
