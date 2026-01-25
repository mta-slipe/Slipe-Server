using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System.Collections.Generic;

namespace SlipeServer.Server.Services;

/// <summary>
/// Service to allow you to add and remove commands, these commands can then be handled by their `Triggered` event
/// </summary>
public class CommandService : ICommandService
{
    private readonly Dictionary<string, HashSet<Command>> caseSensitiveHandlers = [];
    private readonly Dictionary<string, HashSet<Command>> caseInsensitiveHandlers = [];

    public CommandService(MtaServer server)
    {
        server.PlayerJoined += HandlePlayerJoin;
    }

    private void HandlePlayerJoin(Player player)
    {
        player.CommandEntered += HandlePlayerCommand;
    }

    private void HandlePlayerCommand(Player sender, PlayerCommandEventArgs args)
    {
        if (this.caseSensitiveHandlers.TryGetValue(args.Command, out var handlers))
            foreach (var handler in handlers)
                handler.Trigger(args.Source, args.Arguments);

        if (this.caseInsensitiveHandlers.TryGetValue(args.Command.ToLower(), out var insensitiveHandlers))
            foreach (var handler in insensitiveHandlers)
                handler.Trigger(args.Source, args.Arguments);
    }

    public Command AddCommand(string command, bool isCaseSensitive = true)
    {
        var handlerSet = isCaseSensitive ? this.caseSensitiveHandlers : this.caseInsensitiveHandlers;
        if (!isCaseSensitive)
            command = command.ToLower();

        var handler = new Command(command, isCaseSensitive);
        if (!handlerSet.ContainsKey(command))
            handlerSet[command] = [];

        handlerSet[command].Add(handler);
        return handler;
    }

    public void RemoveCommand(Command command)
    {
        var handlerSet = command.IsCaseSensitive ? this.caseSensitiveHandlers : this.caseInsensitiveHandlers;

        if (handlerSet.TryGetValue(command.CommandText, out var handlers))
        {
            handlers.Remove(command);
            if (handlers.Count == 0)
                handlerSet.Remove(command.CommandText);
        }
    }
}
