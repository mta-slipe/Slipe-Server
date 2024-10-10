using SlipeServer.Server.Elements;

namespace SlipeServer.Scripting.Definitions;

public class EventScriptDefinitions
{
    private readonly IScriptEventRuntime eventRuntime;

    public EventScriptDefinitions(IScriptEventRuntime eventRuntime)
    {
        this.eventRuntime = eventRuntime;
    }

    [ScriptFunctionDefinition("addEventHandler")]
    public bool AddEventHandler(string eventName, Element attachedTo, EventDelegate callback)
    {
        this.eventRuntime.AddEventHandler(eventName, attachedTo, callback);
        return true;
    }

    [ScriptFunctionDefinition("removeEventHandler")]
    public bool RemoveEventHandler(string eventName, Element attachedTo, EventDelegate callback)
    {
        this.eventRuntime.RemoveEventHandler(eventName, attachedTo, callback);
        return true;
    }
}
