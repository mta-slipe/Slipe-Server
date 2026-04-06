using SlipeServer.Scripting.EventDefinitions;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Resources;
using System;
using System.Collections.Generic;

namespace SlipeServer.Scripting;

public interface IScriptEventRuntime
{
    void AddEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate, object? owner = null);
    void RemoveEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate);
    void RemoveEventHandlersOwnedBy(object owner);
    void RemoveEventHandlersWithContext(Resource? owningResource);
    void LoadEvents(IEventDefinitions eventDefinitions);
    void LoadDefaultEvents();
    void RegisterEvent<T>(string eventName, EventRegistrationDelegate<T> eventDelegate) where T : Element;

    void AddCustomEvent(string eventName, bool allowRemoteTrigger = false);
    bool TriggerCustomEvent(string eventName, Element element, params object[] arguments);
    void CancelEvent(bool cancel = true, string reason = "");
    bool WasEventCancelled();
    string GetCancelReason();
    IEnumerable<EventDelegate> GetEventHandlers(string eventName, Element attachedTo);
}

public delegate void EventDelegate(Element element, params object[] parameters);
public delegate EventHandlerActions<T> EventRegistrationDelegate<T>(ScriptCallbackDelegateWrapper callback) where T : Element;

public interface IEventHandlerActions
{
    public Action<Element> Add { get; }
    public Action<Element> Remove { get; }
}

public struct EventHandlerActions<T> : IEventHandlerActions where T : Element
{
    public Action<T> Add { get; set; }
    public Action<T> Remove { get; set; }

    readonly Action<Element> IEventHandlerActions.Add { 
        get
        {
            var self = this;
            return (e) => self.Add((T)e);
        } 
    }

    readonly Action<Element> IEventHandlerActions.Remove
    {
        get
        {
            var self = this;
            return (e) => self.Remove((T)e);
        }
    }
}
