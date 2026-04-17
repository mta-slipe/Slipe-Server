using SlipeServer.Scripting.EventDefinitions;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SlipeServer.Server.Resources;
using Microsoft.Extensions.Logging;
using SlipeServer.Packets.Definitions.Lua;

namespace SlipeServer.Scripting;

public class ScriptEventRuntime : IScriptEventRuntime
{
    private readonly List<RegisteredEventHandler> registeredEventHandlers = [];
    private readonly Dictionary<string, RegisteredEvent> registeredEvents = [];
    private readonly Dictionary<string, bool> customEvents = [];
    private readonly List<CustomEventHandlerEntry> customEventHandlers = [];
    private readonly IMtaServer server;
    private readonly IElementCollection elementCollection;
    private readonly ILogger<ScriptEventRuntime> logger;
    private bool lastEventCancelled = false;
    private string lastCancelReason = string.Empty;
    private bool defaultEventsLoaded;

    private readonly Lock handlerLock = new();

    public ScriptEventRuntime(IMtaServer server, IElementCollection elementCollection, ILogger<ScriptEventRuntime> logger)
    {
        this.server = server;
        this.elementCollection = elementCollection;
        this.logger = logger;
        this.server.ElementCreated += HandleElementCreation;
        this.server.LuaEventTriggered += HandleRemoteLuaEvent;
    }

    private void HandleRemoteLuaEvent(LuaEvent luaEvent)
    {
        if (!this.customEvents.TryGetValue(luaEvent.Name, out bool allowRemoteTrigger) || !allowRemoteTrigger)
            return;

        TriggerCustomEventFromClient(luaEvent.Name, luaEvent.Source, luaEvent.Player, luaEvent.Parameters.Cast<object>().ToArray());
    }

    private void HandleElementCreation(Element element)
    {
        lock (this.handlerLock)
        {
            //var handlers = this.registeredEventHandlers
            //    .Where(handler => handler.GetType().IsAssignableFrom(element.GetType()));

            var handlers = this.registeredEventHandlers
                .Where(x => element == x.AttachedTo || (element.IsChildOf(x.AttachedTo) && x.AllowPropagation))
                .Where(x => x.RegisteredEvent.ElementType.IsAssignableFrom(element.GetType()));

            foreach (var handler in handlers)
            {
                var actions = (IEventHandlerActions)handler.RegisteredEvent.Delegate.DynamicInvoke(handler.CallbackWrapper)!;
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
        {
            if (this.customEvents.ContainsKey(eventName))
            {
                lock (this.handlerLock)
                {
                    this.customEventHandlers.Add(new CustomEventHandlerEntry
                    {
                        EventName = eventName,
                        AttachedTo = attachedTo,
                        Delegate = callbackDelegate,
                        Owner = owner,
                        ExecutionContext = ScriptExecutionContext.Current
                    });
                }
                return;
            }

            this.logger.LogError("Attempt to add event handler for non-existent event {eventName}", eventName);

            throw new Exception($"Attempt to add event handler for non-existent event {eventName}");
        }

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
                Owner = owner,
                ExecutionContext = ScriptExecutionContext.Current,
                CallbackWrapper = wrapper
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
            if (this.customEvents.ContainsKey(eventName))
            {
                var customHandlers = this.customEventHandlers
                    .Where(x => x.EventName == eventName && x.Delegate == callbackDelegate && x.AttachedTo == attachedTo)
                    .ToArray();
                foreach (var handler in customHandlers)
                    this.customEventHandlers.Remove(handler);
                return;
            }

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

            var customHandlers = this.customEventHandlers
                .Where(x => x.Owner == owner)
                .ToArray();

            foreach (var handler in customHandlers)
                this.customEventHandlers.Remove(handler);
        }
    }

    public void RemoveEventHandlersWithContext(Resource? owningResource)
    {
        lock (this.handlerLock)
        {
            var handlers = this.registeredEventHandlers
                .Where(x => x.ExecutionContext?.Owner == owningResource)
                .ToArray();

            foreach (var handler in handlers)
            {
                foreach (var action in handler.Actions)
                    action.Remove();

                this.registeredEventHandlers.Remove(handler);
            }

            var customHandlers = this.customEventHandlers
                .Where(x => x.ExecutionContext?.Owner == owningResource)
                .ToArray();

            foreach (var handler in customHandlers)
                this.customEventHandlers.Remove(handler);
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
        if (this.defaultEventsLoaded)
            return;
        this.defaultEventsLoaded = true;

        foreach (var type in typeof(ScriptEventRuntime).Assembly.DefinedTypes
            .Where(type => typeof(IEventDefinitions).IsAssignableFrom(type) && type.IsClass))
        {
            LoadEvents((this.server.Instantiate(type) as IEventDefinitions)!);
        }
    }

    public void AddCustomEvent(string eventName, bool allowRemoteTrigger = false)
    {
        lock (this.handlerLock)
        {
            this.customEvents[eventName] = allowRemoteTrigger;
        }
    }

    public bool TriggerCustomEvent(string eventName, Element element, params object[] arguments)
    {
        this.lastEventCancelled = false;
        this.lastCancelReason = string.Empty;

        CustomEventHandlerEntry[] handlers;
        lock (this.handlerLock)
        {
            handlers = this.customEventHandlers
                .Where(h => h.EventName == eventName &&
                    (h.AttachedTo == element || element.IsChildOf(h.AttachedTo)))
                .ToArray();
        }

        foreach (var handler in handlers)
        {
            handler.Delegate(element, arguments);
        }

        return !this.lastEventCancelled;
    }

    public bool TriggerCustomEventFromClient(string eventName, Element element, Player client, params object[] arguments)
    {
        this.lastEventCancelled = false;
        this.lastCancelReason = string.Empty;

        CustomEventHandlerEntry[] handlers;
        lock (this.handlerLock)
        {
            handlers = this.customEventHandlers
                .Where(h => h.EventName == eventName &&
                    (h.AttachedTo == element || element.IsChildOf(h.AttachedTo)))
                .ToArray();
        }

        arguments = arguments
            .Select(x =>
            {
                if (x is LuaValue luaValue && luaValue.ElementId is not null)
                    return this.elementCollection.Get(luaValue.ElementId.Value) ?? x;

                return x;
            }).ToArray();

        foreach (var handler in handlers)
        {
            ScriptExecutionContext.PendingGlobals = new Dictionary<string, object>
            {
                ["client"] = client
            };
            try
            {
                handler.Delegate(element, arguments);
            }
            finally
            {
                ScriptExecutionContext.PendingGlobals = null;
            }
        }

        return !this.lastEventCancelled;
    }

    public void CancelEvent(bool cancel = true, string reason = "")
    {
        this.lastEventCancelled = cancel;
        this.lastCancelReason = reason;
    }

    public bool WasEventCancelled() => this.lastEventCancelled;

    public string GetCancelReason() => this.lastCancelReason;

    public IEnumerable<EventDelegate> GetEventHandlers(string eventName, Element attachedTo)
    {
        lock (this.handlerLock)
        {
            return this.customEventHandlers
                .Where(h => h.EventName == eventName && h.AttachedTo == attachedTo)
                .Select(h => h.Delegate)
                .ToList();
        }
    }
}

internal struct RegisteredEvent
{
    public string EventName { get; set; }
    public Type ElementType { get; set; }
    public Delegate Delegate { get; set; }
}

internal readonly struct RegisteredEventHandler
{
    public RegisteredEventHandler()
    {

    }

    public required string EventName { get; init; }
    public required RegisteredEvent RegisteredEvent { get; init; }
    public required EventDelegate Delegate { get; init; }
    public required Element AttachedTo { get; init; }
    public List<RegisteredEventHandlerElement> Actions { get; init; } = [];
    public required ScriptCallbackDelegateWrapper CallbackWrapper { get; init; }

    public bool AllowPropagation { get; init; } = true;

    public object? Owner { get; init; }
    public ScriptExecutionContext? ExecutionContext { get; init; }
}


internal readonly struct RegisteredEventHandlerElement
{
    public Element Element { get; init; }
    public IEventHandlerActions Actions { get; init; }

    public readonly void Remove() => this.Actions.Remove(this.Element);
}

internal readonly struct CustomEventHandlerEntry
{
    public string EventName { get; init; }
    public Element AttachedTo { get; init; }
    public EventDelegate Delegate { get; init; }
    public object? Owner { get; init; }
    public required ScriptExecutionContext? ExecutionContext { get; init; }
}
