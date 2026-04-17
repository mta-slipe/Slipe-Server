using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting.Definitions;

public class EventScriptDefinitions(
    IScriptEventRuntime eventRuntime,
    ILuaEventService luaEventService,
    IElementCollection elementCollection,
    object? owner = null)
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

    [ScriptFunctionDefinition("addEvent")]
    public bool AddEvent(string eventName, bool allowRemoteTrigger = false)
    {
        eventRuntime.AddCustomEvent(eventName, allowRemoteTrigger);
        return true;
    }

    [ScriptFunctionDefinition("triggerEvent")]
    public bool TriggerEvent(string eventName, Element baseElement, params LuaValue[] arguments)
    {
        return eventRuntime.TriggerCustomEvent(eventName, baseElement, arguments.Cast<object>().ToArray());
    }

    [ScriptFunctionDefinition("cancelEvent")]
    public bool CancelEvent(bool cancel = true, string reason = "")
    {
        eventRuntime.CancelEvent(cancel, reason);
        return true;
    }

    [ScriptFunctionDefinition("wasEventCancelled")]
    public bool WasEventCancelled()
    {
        return eventRuntime.WasEventCancelled();
    }

    [ScriptFunctionDefinition("getCancelReason")]
    public string GetCancelReason()
    {
        return eventRuntime.GetCancelReason();
    }

    [ScriptFunctionDefinition("getEventHandlers")]
    public IEnumerable<EventDelegate> GetEventHandlers(string eventName, Element attachedTo)
    {
        return eventRuntime.GetEventHandlers(eventName, attachedTo);
    }

    [ScriptFunctionDefinition("triggerClientEvent")]
    public bool TriggerClientEvent(Element? sendTo, string eventName, Element sourceElement, params LuaValue[] arguments)
    {
        if (sendTo is Player player)
            luaEventService.TriggerEventFor(player, eventName, sourceElement, arguments);
        else
            luaEventService.TriggerEvent(eventName, sourceElement, arguments);
        return true;
    }

}
