using SlipeServer.Scripting.EventDefinitions;
using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Scripting;

public interface IScriptEventRuntime
{
    void AddEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate, object? owner = null);
    void RemoveEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate);
    void RemoveEventHandlersOwnedBy(object owner);
    void LoadEvents(IEventDefinitions eventDefinitions);
    void LoadDefaultEvents();
    void RegisterEvent<T>(string eventName, EventRegistrationDelegate<T> eventDelegate) where T : Element;
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
