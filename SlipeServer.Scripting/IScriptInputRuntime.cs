using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using System.Collections.Generic;

namespace SlipeServer.Scripting;

public interface IScriptInputRuntime
{
    void AddCommandHandler(string commandName, ScriptCallbackDelegateWrapper callbackDelegate);
    void RemoveCommandHandler(string commandName, ScriptCallbackDelegateWrapper? callbackDelegate = null);
    void ExecuteCommandHandler(string commandName, Player player, string? args = null);
    IEnumerable<string> GetCommandHandlers();

    void BindKey(Player player, string key, KeyState keyState, ScriptCallbackDelegateWrapper callback);
    void UnbindKey(Player player, string key, KeyState? keyState = null, ScriptCallbackDelegateWrapper? callback = null);
    bool IsKeyBound(Player player, string key, KeyState? keyState = null, ScriptCallbackDelegateWrapper? handler = null);
    IEnumerable<object> GetFunctionsBoundToKey(Player player, string key, KeyState keyState);
    string? GetKeyBoundToFunction(Player player, ScriptCallbackDelegateWrapper handler);
}
