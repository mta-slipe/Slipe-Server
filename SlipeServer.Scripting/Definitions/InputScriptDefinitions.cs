using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using System.Collections.Generic;

namespace SlipeServer.Scripting.Definitions;

public class InputScriptDefinitions(IScriptInputRuntime inputRuntime)
{
    private static KeyState ParseKeyState(string keyState) => keyState.ToLower() switch
    {
        "down" => KeyState.Down,
        "up" => KeyState.Up,
        "both" => KeyState.Both,
        _ => KeyState.Down
    };

    [ScriptFunctionDefinition("addCommandHandler")]
    public void AddCommandHandler(string commandName, ScriptCallbackDelegateWrapper callback, bool restricted = false, bool caseSensitive = true)
    {
        inputRuntime.AddCommandHandler(commandName, callback);
    }

    [ScriptFunctionDefinition("removeCommandHandler")]
    public void RemoveCommandHandler(string commandName, ScriptCallbackDelegateWrapper? callback = null)
    {
        inputRuntime.RemoveCommandHandler(commandName, callback);
    }

    [ScriptFunctionDefinition("executeCommandHandler")]
    public bool ExecuteCommandHandler(string commandName, Player player, string? args = null)
    {
        inputRuntime.ExecuteCommandHandler(commandName, player, args);
        return true;
    }

    [ScriptFunctionDefinition("getCommandHandlers")]
    public IEnumerable<string> GetCommandHandlers()
    {
        return inputRuntime.GetCommandHandlers();
    }

    [ScriptFunctionDefinition("bindKey")]
    public bool BindKey(Player player, string key, string keyState, ScriptCallbackDelegateWrapper callback)
    {
        inputRuntime.BindKey(player, key, ParseKeyState(keyState), callback);
        return true;
    }

    [ScriptFunctionDefinition("unbindKey")]
    public bool UnbindKey(Player player, string key, string? keyState = null, ScriptCallbackDelegateWrapper? callback = null)
    {
        var ks = keyState != null ? ParseKeyState(keyState) : (KeyState?)null;
        inputRuntime.UnbindKey(player, key, ks, callback);
        return true;
    }

    [ScriptFunctionDefinition("isKeyBound")]
    public bool IsKeyBound(Player player, string key, string? keyState = null, ScriptCallbackDelegateWrapper? handler = null)
    {
        var ks = keyState != null ? ParseKeyState(keyState) : (KeyState?)null;
        return inputRuntime.IsKeyBound(player, key, ks, handler);
    }

    [ScriptFunctionDefinition("getFunctionsBoundToKey")]
    public IEnumerable<object> GetFunctionsBoundToKey(Player player, string key, string keyState)
    {
        return inputRuntime.GetFunctionsBoundToKey(player, key, ParseKeyState(keyState));
    }

    [ScriptFunctionDefinition("getKeyBoundToFunction")]
    public string? GetKeyBoundToFunction(Player player, ScriptCallbackDelegateWrapper handler)
    {
        return inputRuntime.GetKeyBoundToFunction(player, handler);
    }

    [ScriptFunctionDefinition("isControlEnabled")]
    public bool IsControlEnabled(Player player, string control)
    {
        return player.Controls.IsEnabled(control);
    }

    [ScriptFunctionDefinition("toggleControl")]
    public bool ToggleControl(Player player, string control, bool enabled)
    {
        player.Controls.SetEnabled(control, enabled);
        return true;
    }

    [ScriptFunctionDefinition("toggleAllControls")]
    public bool ToggleAllControls(Player player, bool enabled, bool gtaControls = true, bool mtaControls = true)
    {
        player.Controls.ToggleAll(enabled, gtaControls, mtaControls);
        return true;
    }
}
