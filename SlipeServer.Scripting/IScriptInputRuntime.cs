namespace SlipeServer.Scripting;

public interface IScriptInputRuntime
{
    void AddCommandHandler(string eventName, ScriptCallbackDelegateWrapper callbackDelegate);
    void RemoveCommandHandler(string eventName, ScriptCallbackDelegateWrapper? callbackDelegate = null);
}
