using SlipeServer.Scripting.EventDefinitions;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting;

public class SimplifiedScriptEventRuntime : IScriptEventRuntime
{
    private readonly MtaServer server;
    private readonly IElementCollection elementCollection;

    private readonly List<RegisteredEventHandler> registeredEventHandlers;
    private readonly Dictionary<string, RegisteredEvent> registeredEvents;

    public SimplifiedScriptEventRuntime(MtaServer server, IElementCollection elementCollection)
    {
        this.server = server;
        this.elementCollection = elementCollection;

        this.registeredEventHandlers = new List<RegisteredEventHandler>();
        this.registeredEvents = new Dictionary<string, RegisteredEvent>();

        this.server.ElementCreated += HandleElementCreation;


        //this.RegisterEvent<Player>((element, handler) => element.Destroyed += handler, (element, handler) => element.Destroyed -= handler);
    }

    private void HandleElementCreation(Element element)
    {

    }

    private void RegisterEvent<T>(Action<T, HandleEventDelegate> eventSubscriber, Action<T, HandleEventDelegate> eventUnsubscriber)
    {

    }

    public void AddEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate)
    {
        var serverResource = ServerResourceContext.Current;

        if (serverResource == null)
            throw new InvalidOperationException("Can not add event outside resource.");

        if (!this.registeredEvents.ContainsKey(eventName))
            return;

        var registeredEvent = this.registeredEvents[eventName];
        var registeredEventHandler = new RegisteredEventHandler
        {
            EventName = eventName,
            RegisteredEvent = registeredEvent,
            Delegate = callbackDelegate,
            Element = attachedTo,
            ServerResource = serverResource
        };

        this.registeredEventHandlers.Add(registeredEventHandler);

        foreach (var element in this.elementCollection.GetAll())
        {
            if (registeredEvent.ElementType.IsAssignableFrom(element.GetType()))
            {
                var actions = (EventHandlerActions<Element>)registeredEvent.Delegate.DynamicInvoke(element, callbackDelegate)!;
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

    public void HandleEvent(Element source, string eventName, params object[] parameters)
    {
        var handlers = this.registeredEventHandlers.Where(e => e.EventName == eventName);
        foreach (var handler in handlers)
        {
            handler.Delegate.DynamicInvoke(source, parameters);
        }
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

    public void TriggerEvent(string eventName, Element sourceElement, Element baseElement, params object[] parameters) => throw new NotImplementedException();
    public void RemoveAllRootElementEvents(ServerResource serverResource) => throw new NotImplementedException();
}

public delegate void HandleEventDelegate(Element element, string eventName, params object[] parameters);
