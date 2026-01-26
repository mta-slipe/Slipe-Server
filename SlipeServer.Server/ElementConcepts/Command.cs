using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using System;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents a command a player can use to execute an action
/// </summary>
public class Command(string commandText, bool isCaseSensitive)
{
    public string CommandText { get; set; } = commandText;
    public bool IsCaseSensitive { get; } = isCaseSensitive;

    public void Trigger(Player player, string[] arguments) => this.Triggered?.Invoke(this, new(player, arguments));

    public event EventHandler<CommandTriggeredEventArgs>? Triggered;
}
