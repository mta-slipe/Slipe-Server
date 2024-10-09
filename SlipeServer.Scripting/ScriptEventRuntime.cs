using SlipeServer.Scripting.EventDefinitions;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting;

public class ScriptEventRuntime : IScriptEventRuntime
{
    private readonly List<RegisteredEventHandler> registeredEventHandlers;
    private readonly Dictionary<string, RegisteredEvent> registeredEvents;
    private readonly MtaServer server;
    private readonly IElementCollection elementCollection;

    public ScriptEventRuntime(MtaServer server, IElementCollection elementCollection)
    {
        this.registeredEventHandlers = [];
        this.registeredEvents = [];

        this.server = server;
        this.elementCollection = elementCollection;
        this.server.ElementCreated += HandleElementCreation;
    }

    private void HandleElementCreation(Element element)
    {
        var handlers = this.registeredEventHandlers.Where(handler => handler.GetType().IsAssignableFrom(element.GetType()));
        foreach (var handler in handlers)
        {
            var actions = (EventHandlerActions<Element>)handler.RegisteredEvent.Delegate.DynamicInvoke(element, handler.Delegate)!;
            actions.Add(element);
        }

        element.Destroyed += HandleElementDestruction;
    }

    private void HandleElementDestruction(Element element)
    {
        var handlers = this.registeredEventHandlers.RemoveAll(handler => handler.Element == element);
    }

    public void AddEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate)
    {
        if (!this.registeredEvents.ContainsKey(eventName))
            return;

        var registeredEvent = this.registeredEvents[eventName];
        var registeredEventHandler = new RegisteredEventHandler()
        {
            EventName = eventName,
            RegisteredEvent = registeredEvent,
            Delegate = callbackDelegate,
            Element = attachedTo,
        };

        this.registeredEventHandlers.Add(registeredEventHandler);

        void elementCheckingDelegate(object[] objects)
        {
            if (objects.First() is Element element)
                if (element != attachedTo && !element.IsChildOf(attachedTo))
                    return;

            if (objects.Length == 0)
            {
                callbackDelegate.DynamicInvoke(null, Array.Empty<object>());
            } else if (objects.Length == 1)
            {
                callbackDelegate.DynamicInvoke(objects.First(), Array.Empty<object>());
            } else
            {
                callbackDelegate.DynamicInvoke(objects.First(), objects.Skip(1));
            }
        }

        ScriptCallbackDelegateWrapper wrapper = new ScriptCallbackDelegateWrapper(elementCheckingDelegate, new());

        foreach (var element in this.elementCollection.GetAll())
        {
            if (registeredEvent.ElementType.IsAssignableFrom(element.GetType()))
            {
                var actions = (EventHandlerActions<Element>)registeredEvent.Delegate.DynamicInvoke(element, wrapper)!;
                actions.Add(element);
            }
        }
    }

    public void RemoveEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate)
    {
        var registeredEvent = this.registeredEvents[eventName];
        var handlers = this.registeredEventHandlers.Where(x => x.EventName == eventName && x.Delegate == callbackDelegate && x.Element == attachedTo);

        foreach (var handler in handlers)
        {
            this.registeredEventHandlers.Remove(handler);
        }
    }

    public void RegisterEvent<T>(string eventName, EventRegistrationDelegate<T> eventDelegate) where T : Element
    {
        this.registeredEvents[eventName] = new RegisteredEvent()
        {
            ElementType = typeof(T),
            EventName = eventName,
            Delegate = eventDelegate,
        };
    }

    public void LoadEvents(IEventDefinitions eventDefinitions)
    {
        eventDefinitions.LoadInto(this);
    }

    public void LoadDefaultEvents()
    {
        foreach (var type in typeof(ScriptEventRuntime).Assembly.DefinedTypes
            .Where(type => typeof(IEventDefinitions).IsAssignableFrom(type) && type.IsClass))
        {
            LoadEvents((this.server.Instantiate(type) as IEventDefinitions)!);
        }
    }
}

internal struct RegisteredEvent
{
    public string EventName { get; set; }
    public Type ElementType { get; set; }
    public Delegate Delegate { get; set; }
}

internal struct RegisteredEventHandler
{
    public string EventName { get; set; }
    public RegisteredEvent RegisteredEvent { get; set; }
    public EventDelegate Delegate { get; set; }
    public Element Element { get; set; }
}
