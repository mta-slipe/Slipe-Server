using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System.Collections.Generic;

namespace SlipeServer.Scripting;

public class ScriptInputRuntime : IScriptInputRuntime
{
    private readonly List<RegisteredCommandHandler> registeredCommandHandlers;
    private readonly MtaServer server;

    public ScriptInputRuntime(MtaServer server, IElementCollection elementCollection)
    {
        this.registeredCommandHandlers = new List<RegisteredCommandHandler>();

        this.server = server;
        this.server.PlayerJoined += HandlePlayerJoined;

        foreach (var player in elementCollection.GetByType<Player>())
        {
            HandlePlayerJoined(player);
        }
    }

    private void HandlePlayerJoined(Player player)
    {
        player.CommandEntered += CommandEntered;
        player.Destroyed += Destroyed;
    }

    private void Destroyed(Element obj)
    {
        (obj as Player)!.CommandEntered -= CommandEntered;
    }

    private void CommandEntered(Player player, PlayerCommandEventArgs e)
    {
        foreach (var commandHandler in this.registeredCommandHandlers)
        {
            if (commandHandler.CommandName == e.Command)
            {
                commandHandler.Delegate.CallbackDelegate.Invoke(player, e.Command, e.Arguments);
            }
        }
    }

    public void AddCommandHandler(string commandName, ScriptCallbackDelegateWrapper callbackDelegate)
    {
        this.registeredCommandHandlers.Add(new RegisteredCommandHandler
        {
            CommandName = commandName,
            Delegate = callbackDelegate,
        });
    }

    public void RemoveCommandHandler(string commandName, ScriptCallbackDelegateWrapper? callbackDelegate = null)
    {
        if (callbackDelegate == null)
            this.registeredCommandHandlers.RemoveAll(x => x.CommandName == commandName);
        else
            this.registeredCommandHandlers.RemoveAll(x => x.CommandName == commandName && x.Delegate.Equals(callbackDelegate));
    }
}

internal struct RegisteredCommandHandler
{
    public string CommandName { get; set; }
    public ScriptCallbackDelegateWrapper Delegate { get; set; }
}
