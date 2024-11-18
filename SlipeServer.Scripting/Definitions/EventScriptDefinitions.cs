using SlipeServer.Server.Elements;

namespace SlipeServer.Scripting.Definitions;

public class EventScriptDefinitions(IScriptEventRuntime eventRuntime, object? owner = null)
{
    [ScriptFunctionDefinition("addEventHandler")]
    public void AddEventHandler(string eventName, Element attachedTo, EventDelegate callback)
    {
        eventRuntime.AddEventHandler(eventName, attachedTo, callback, owner);
    }

    [ScriptFunctionDefinition("removeEventHandler")]
    public void RemoveEventHandler(string eventName, Element attachedTo, EventDelegate callback)
    {
        eventRuntime.RemoveEventHandler(eventName, attachedTo, callback);
    }
}
