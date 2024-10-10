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
    void TriggerEvent(string eventName, Element sourceElement, Element baseElement, params object[] parameters);
    void RemoveAllRootElementEvents(ServerResource serverResource);
}

public delegate void EventDelegate(Element element, params object[] parameters);
public delegate EventHandlerActions<T> EventRegistrationDelegate<T>(Element element, ScriptCallbackDelegateWrapper callback) where T : Element;

public struct EventHandlerActions<T> where T : Element
{
    public Action<T> Add { get; set; }
    public Action<T> Remove { get; set; }
}
