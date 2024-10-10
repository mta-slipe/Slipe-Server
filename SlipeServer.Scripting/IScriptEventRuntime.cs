using SlipeServer.Scripting.EventDefinitions;
using SlipeServer.Server.Elements;
using System;

namespace SlipeServer.Scripting;

public interface IScriptEventRuntime
{
    void AddEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate);
    void RemoveEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate);
    void LoadEvents(IEventDefinitions eventDefinitions);
    void LoadDefaultEvents();
    void RegisterEvent<T>(string eventName, EventRegistrationDelegate<T> eventDelegate) where T : Element;
}

public delegate void EventDelegate(Element element, params object[] parameters);
public delegate EventHandlerActions<T> EventRegistrationDelegate<T>(Element element, ScriptCallbackDelegateWrapper callback) where T : Element;


public interface IEventHandlerActions
{
    public Action<Element> Add { get; }
    public Action<Element> Remove { get; }
}


public struct EventHandlerActions<T> : IEventHandlerActions where T : Element
{
    public Action<T> Add { get; set; }
    public Action<T> Remove { get; set; }

    Action<Element> IEventHandlerActions.Add { 
        get
        {
            var self = this;
            return (e) => self.Add((T)e);
        } 
    }
    Action<Element> IEventHandlerActions.Remove
    {
        get
        {
            var self = this;
            return (e) => self.Remove((T)e);
        }
    }
}
