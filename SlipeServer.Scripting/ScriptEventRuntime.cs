using SlipeServer.Scripting.EventDefinitions;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SlipeServer.Scripting;

public class ScriptEventRuntime : IScriptEventRuntime
{
    private readonly List<RegisteredEventHandler> registeredEventHandlers = [];
    private readonly Dictionary<string, RegisteredEvent> registeredEvents = [];
    private readonly IMtaServer server;
    private readonly IElementCollection elementCollection;

    private readonly Lock handlerLock = new();

    public ScriptEventRuntime(IMtaServer server, IElementCollection elementCollection)
    {
        this.server = server;
        this.elementCollection = elementCollection;
        this.server.ElementCreated += HandleElementCreation;
    }

    private void HandleElementCreation(Element element)
    {
        lock (this.handlerLock)
        {
            var handlers = this.registeredEventHandlers.Where(handler => handler.GetType().IsAssignableFrom(element.GetType()));
            foreach (var handler in handlers)
            {
                var actions = (IEventHandlerActions)handler.RegisteredEvent.Delegate.DynamicInvoke(handler.Delegate)!;
                actions.Add(element);

                handler.Actions.Add(new RegisteredEventHandlerElement()
                {
                    Element = element,
                    Actions = actions
                });
            }

            element.Destroyed += HandleElementDestruction;
        }
    }

    private void HandleElementDestruction(Element element)
    {
        lock (this.handlerLock)
        {
            var attachedHandlers = this.registeredEventHandlers
                .Where(handler => handler.AttachedTo == element)
                .ToArray();

            foreach (var handler in attachedHandlers)
            {
                foreach (var action in handler.Actions)
                    action.Remove();

                this.registeredEventHandlers.Remove(handler);
            }

            var relatedHandlers = this.registeredEventHandlers
                .Where(handler => handler.Actions.Any(x => x.Element == element))
                .ToArray();

            foreach (var handler in relatedHandlers)
            {
                var action = handler.Actions.Single(x => x.Element == element);
                action.Remove();
                handler.Actions.Remove(action);
            }
        }
    }

    public void AddEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate, object? owner = null)
    {
        if (!this.registeredEvents.TryGetValue(eventName, out var registeredEvent))
            throw new Exception($"Attempt to add event handler for non-existent event {eventName}");

        lock (this.handlerLock)
        {
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
                    callbackDelegate.DynamicInvoke(objects.First(), objects.Skip(1).ToArray());
                }
            }

            ScriptCallbackDelegateWrapper wrapper = new ScriptCallbackDelegateWrapper(elementCheckingDelegate, new());

            var registeredEventHandler = new RegisteredEventHandler()
            {
                EventName = eventName,
                RegisteredEvent = registeredEvent,
                Delegate = callbackDelegate,
                AttachedTo = attachedTo,
                Actions = [],
                Owner = owner
            };

            this.registeredEventHandlers.Add(registeredEventHandler);

            foreach (var element in this.elementCollection.GetAll())
            {
                if (registeredEvent.ElementType.IsAssignableFrom(element.GetType()))
                {
                    var actions = (IEventHandlerActions)registeredEvent.Delegate.DynamicInvoke(wrapper)!;
                    actions.Add(element);

                    registeredEventHandler.Actions.Add(new RegisteredEventHandlerElement()
                    {
                        Element = element,
                        Actions = actions
                    });
                }
            }
        }
    }

    public void RemoveEventHandler(string eventName, Element attachedTo, EventDelegate callbackDelegate)
    {
        lock (this.handlerLock)
        {
            var registeredEvent = this.registeredEvents[eventName];
            var handlers = this.registeredEventHandlers
                .Where(x => x.EventName == eventName && x.Delegate == callbackDelegate && x.AttachedTo == attachedTo)
                .ToArray();

            foreach (var handler in handlers)
            {
                foreach (var action in handler.Actions)
                    action.Remove();

                this.registeredEventHandlers.Remove(handler);
            }
        }
    }

    public void RemoveEventHandlersOwnedBy(object owner)
    {
        lock (this.handlerLock)
        {
            var handlers = this.registeredEventHandlers
                .Where(x => x.Owner == owner)
                .ToArray();

            foreach (var handler in handlers)
            {
                foreach (var action in handler.Actions)
                    action.Remove();

                this.registeredEventHandlers.Remove(handler);
            }
        }
    }

    public void RegisterEvent<T>(string eventName, EventRegistrationDelegate<T> eventDelegate) where T : Element
    {
        lock (this.handlerLock)
        {
            this.registeredEvents[eventName] = new RegisteredEvent()
            {
                ElementType = typeof(T),
                EventName = eventName,
                Delegate = eventDelegate,
            };
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
}

internal struct RegisteredEvent
{
    public string EventName { get; set; }
    public Type ElementType { get; set; }
    public Delegate Delegate { get; set; }
}

internal struct RegisteredEventHandler
{
    public string EventName { get; init; }
    public RegisteredEvent RegisteredEvent { get; init; }
    public EventDelegate Delegate { get; init; }
    public Element AttachedTo { get; init; }
    public List<RegisteredEventHandlerElement> Actions { get; init; }

    public object? Owner { get; init; }
}


internal readonly struct RegisteredEventHandlerElement
{
    public Element Element { get; init; }
    public IEventHandlerActions Actions { get; init; }

    public readonly void Remove() => this.Actions.Remove(this.Element);
}
