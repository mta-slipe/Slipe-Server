using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Server.Events;

public sealed class CommandTriggeredEventArgs(Player player, string[] arguments) : EventArgs
{
    public Player Player { get; } = player;
    public string[] Arguments { get; } = arguments;
}
