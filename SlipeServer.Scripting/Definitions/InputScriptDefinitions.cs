namespace SlipeServer.Scripting.Definitions;

public class InputScriptDefinitions(IScriptInputRuntime inputRuntime)
{
    [ScriptFunctionDefinition("addCommandHandler")]
    public void AddCommandHandler(string commandName, ScriptCallbackDelegateWrapper callback)
    {
        inputRuntime.AddCommandHandler(commandName, callback);
    }

    [ScriptFunctionDefinition("removeCommandHandler")]
    public void RemoveCommandHandler(string commandName, ScriptCallbackDelegateWrapper? callback = null)
    {
        inputRuntime.RemoveCommandHandler(commandName, callback);
    }
}
