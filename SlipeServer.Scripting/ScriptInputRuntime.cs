using SlipeServer.Server;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Resources;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting;

public class ScriptInputRuntime : IScriptInputRuntime
{
    private readonly List<RegisteredCommandHandler> registeredCommandHandlers = [];
    private readonly Dictionary<Player, List<RegisteredKeyBinding>> keyBindings = [];
    private readonly IMtaServer server;
    private readonly IScriptEventRuntime? eventRuntime;

    public ScriptInputRuntime(IMtaServer server, IElementCollection elementCollection, IScriptEventRuntime? eventRuntime = null)
    {
        this.server = server;
        this.eventRuntime = eventRuntime;
        this.server.PlayerJoined += HandlePlayerJoined;

        foreach (var player in elementCollection.GetByType<Player>())
        {
            HandlePlayerJoined(player);
        }
    }

    private void HandlePlayerJoined(Player player)
    {
        player.CommandEntered += CommandEntered;
        player.BindExecuted += HandleBindExecuted;
        player.Destroyed += Destroyed;
    }

    private void Destroyed(Element obj)
    {
        if (obj is Player player)
        {
            player.CommandEntered -= CommandEntered;
            player.BindExecuted -= HandleBindExecuted;
            this.keyBindings.Remove(player);
        }
    }

    private void CommandEntered(Player player, PlayerCommandEventArgs e)
    {
        if (this.eventRuntime?.WasEventCancelled() == true)
            return;

        foreach (var commandHandler in this.registeredCommandHandlers)
        {
            if (commandHandler.CommandName == e.Command)
            {
                commandHandler.Delegate.CallbackDelegate.Invoke(player, e.Command, e.Arguments);
            }
        }
    }

    private void HandleBindExecuted(Player player, PlayerBindExecutedEventArgs e)
    {
        if (!this.keyBindings.TryGetValue(player, out var bindings))
            return;

        var keyState = e.KeyState == KeyState.Down ? "down" : "up";

        foreach (var binding in bindings.Where(b =>
            b.Key == e.Key &&
            (b.KeyState == e.KeyState || b.KeyState == KeyState.Both)))
        {
            object?[] args = [player, e.Key, keyState, .. binding.ExtraArguments];
            binding.Delegate.CallbackDelegate.Invoke(args);
        }
    }

    public void AddCommandHandler(string commandName, ScriptCallbackDelegateWrapper callbackDelegate)
    {
        this.registeredCommandHandlers.Add(new RegisteredCommandHandler
        {
            CommandName = commandName,
            Delegate = callbackDelegate,
            ExecutionContext = ScriptExecutionContext.Current,
        });
    }

    public void RemoveCommandHandler(string commandName, ScriptCallbackDelegateWrapper? callbackDelegate = null)
    {
        if (callbackDelegate == null)
            this.registeredCommandHandlers.RemoveAll(x => x.CommandName == commandName);
        else
            this.registeredCommandHandlers.RemoveAll(x => x.CommandName == commandName && x.Delegate.Equals(callbackDelegate));
    }

    public void RemoveCommandHandlersWithContext(Resource? owningResource)
    {
        this.registeredCommandHandlers.RemoveAll(x => x.ExecutionContext?.Owner == owningResource);
    }

    public void RemoveKeyBindingsWithContext(Resource? owningResource)
    {
        foreach (var (player, bindings) in this.keyBindings)
        {
            var affectedKeys = bindings
                .Where(b => b.ExecutionContext?.Owner == owningResource)
                .Select(b => b.Key)
                .Distinct()
                .ToList();

            bindings.RemoveAll(b => b.ExecutionContext?.Owner == owningResource);

            foreach (var key in affectedKeys)
                UpdateClientKeyState(player, key);
        }
    }

    public void ExecuteCommandHandler(string commandName, Player player, string? args = null)
    {
        var arguments = args == null ? [] : args.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var handler in this.registeredCommandHandlers.Where(x => x.CommandName == commandName))
        {
            handler.Delegate.CallbackDelegate.Invoke(player, commandName, arguments);
        }
    }

    public IEnumerable<string> GetCommandHandlers()
    {
        return this.registeredCommandHandlers.Select(x => x.CommandName).Distinct();
    }

    public void BindKey(Player player, string key, KeyState keyState, ScriptCallbackDelegateWrapper callback, params object?[] arguments)
    {
        if (!this.keyBindings.TryGetValue(player, out var bindings))
        {
            bindings = new List<RegisteredKeyBinding>();
            this.keyBindings[player] = bindings;
        }

        bindings.Add(new RegisteredKeyBinding { Key = key, KeyState = keyState, Delegate = callback, ExecutionContext = ScriptExecutionContext.Current, ExtraArguments = arguments });
        UpdateClientKeyState(player, key);
    }

    public void BindKeyCommand(Player player, string key, KeyState keyState, string commandName, string? args = null)
    {
        var wrapper = new ScriptCallbackDelegateWrapper(parameters =>
        {
            var pressingPlayer = parameters.Length > 0 ? parameters[0] as Player ?? player : player;
            ExecuteCommandHandler(commandName, pressingPlayer, args);
        }, commandName);

        BindKey(player, key, keyState, wrapper);
    }

    public void UnbindKey(Player player, string key, KeyState? keyState = null, ScriptCallbackDelegateWrapper? callback = null)
    {
        if (!this.keyBindings.TryGetValue(player, out var bindings))
            return;

        bindings.RemoveAll(b =>
            b.Key == key &&
            (keyState == null || b.KeyState == keyState) &&
            (callback == null || b.Delegate.Equals(callback)));

        UpdateClientKeyState(player, key);
    }

    public bool IsKeyBound(Player player, string key, KeyState? keyState = null, ScriptCallbackDelegateWrapper? handler = null)
    {
        if (!this.keyBindings.TryGetValue(player, out var bindings))
            return false;

        return bindings.Any(b =>
            b.Key == key &&
            (keyState == null || b.KeyState == keyState || b.KeyState == KeyState.Both) &&
            (handler == null || b.Delegate.Equals(handler)));
    }

    public IEnumerable<object> GetFunctionsBoundToKey(Player player, string key, KeyState keyState)
    {
        if (!this.keyBindings.TryGetValue(player, out var bindings))
            return [];

        return bindings
            .Where(b => b.Key == key && (b.KeyState == keyState || b.KeyState == KeyState.Both))
            .Select(b => (object)b.Delegate.CallbackDelegate)
            .ToList();
    }

    public string? GetKeyBoundToFunction(Player player, ScriptCallbackDelegateWrapper handler)
    {
        if (!this.keyBindings.TryGetValue(player, out var bindings))
            return null;

        return bindings.Where(b => b.Delegate.Equals(handler)).Select(b => b.Key).FirstOrDefault();
    }

    private void UpdateClientKeyState(Player player, string key)
    {
        var bindings = this.keyBindings.TryGetValue(player, out var b) ? b : [];
        var relevant = bindings.Where(x => x.Key == key).ToList();

        bool needDown = relevant.Any(x => x.KeyState == KeyState.Down || x.KeyState == KeyState.Both);
        bool needUp = relevant.Any(x => x.KeyState == KeyState.Up || x.KeyState == KeyState.Both);

        KeyState needed = (needDown, needUp) switch
        {
            (true, true) => KeyState.Both,
            (true, false) => KeyState.Down,
            (false, true) => KeyState.Up,
            _ => KeyState.None
        };

        var current = player.BoundKeys.GetValueOrDefault(key, KeyState.None);
        if (needed == current)
            return;

        if (current != KeyState.None)
            player.RemoveBind(key, current);
        if (needed != KeyState.None)
            player.SetBind(key, needed);
    }
}

internal struct RegisteredCommandHandler
{
    public string CommandName { get; set; }
    public ScriptCallbackDelegateWrapper Delegate { get; set; }
    public ScriptExecutionContext? ExecutionContext { get; set; }
}

internal struct RegisteredKeyBinding
{
    public string Key { get; set; }
    public KeyState KeyState { get; set; }
    public ScriptCallbackDelegateWrapper Delegate { get; set; }
    public ScriptExecutionContext? ExecutionContext { get; set; }
    public object?[] ExtraArguments { get; set; }
}
