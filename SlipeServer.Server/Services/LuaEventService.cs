using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Events.Attributes;
using SlipeServer.Server.Events.Interfaces;
using SlipeServer.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SlipeServer.Server.Services
{
    public class LuaEventService
    {
        private readonly MtaServer server;
        private readonly RootElement root;

        private readonly Dictionary<string, List<Action<LuaEvent>>> eventHandlers;
        private readonly Dictionary<ILuaEventHandler, Dictionary<string, Action<LuaEvent>>> luaEventHandlers;

        public LuaEventService(MtaServer server, RootElement root)
        {
            this.server = server;
            this.root = root;
            this.eventHandlers = new Dictionary<string, List<Action<LuaEvent>>>();
            this.luaEventHandlers = new Dictionary<ILuaEventHandler, Dictionary<string, Action<LuaEvent>>>();

            server.LuaEventTriggered += HandleLuaEvent;
        }

        public void TriggerEvent(string eventName, Element? source = null, params LuaValue[] parameters)
        {
            this.server.BroadcastPacket(new LuaEventPacket(eventName, (source ?? this.root).Id, parameters));
        }

        public void TriggerEvent(Player player, string eventName, Element? source = null, params LuaValue[] parameters)
        {
            new LuaEventPacket(eventName, (source ?? this.root).Id, parameters).SendTo(player);
        }

        public void TriggerEvent(Player[] players, string eventName, Element? source = null, params LuaValue[] parameters)
        {
            new LuaEventPacket(eventName, (source ?? this.root).Id, parameters).SendTo(players);
        }

        public void AddEventHandler(string eventName, Action<LuaEvent> handler)
        {
            if (!this.eventHandlers.ContainsKey(eventName))
            {
                this.eventHandlers[eventName] = new List<Action<LuaEvent>>();
            }
            this.eventHandlers[eventName].Add(handler);
        }
        
        public void AddEventHandler<THandler>(THandler handler) where THandler: ILuaEventHandler
        {
            foreach (var pair in this.luaEventHandlers)
            {
                if (handler.GetType() == pair.Key.GetType())
                {
                    return;
                }
            }
            this.luaEventHandlers[handler] = new Dictionary<string, Action<LuaEvent>>();
            foreach (MethodInfo? method in handler.GetType().GetMethods())
            {
                object[] attributes = method.GetCustomAttributes(typeof(EventNameAttribute), false);
                if (attributes.Length == 1)
                {
                    EventNameAttribute eventNameAttribute = (EventNameAttribute)attributes[0];
                    string fullName = handler.BaseName + eventNameAttribute.Name;
                    this.luaEventHandlers[handler][fullName] = ((LuaEvent luaEvent) =>
                    {
                        method.Invoke(handler, new object[] { luaEvent });
                    });
                }
            }
        }
        public void AddEventHandler<THandler>(params object[] vs) where THandler: ILuaEventHandler
        {
            THandler? handler = (THandler)Activator.CreateInstance(typeof(THandler), vs);
            if(handler != null)
            {
                AddEventHandler(handler);
            }
        }

        public void RemoveEventHandler<THandler>(THandler handler) where THandler : ILuaEventHandler
        {
            foreach (var pair in this.luaEventHandlers)
            {
                if ((ILuaEventHandler?)handler == pair.Key)
                {
                    this.luaEventHandlers.Remove(pair.Key);
                    return;
                }
            }
        }

        public void RemoveEventHandler<THandler>() where THandler : ILuaEventHandler
        {
            foreach (var handler in this.luaEventHandlers)
            {
                if (typeof(THandler) == handler.Key.GetType())
                {
                    this.luaEventHandlers.Remove(handler.Key);
                    return;
                }
            }
        }

        public void RemoveEventHandler(string eventName, Action<LuaEvent> handler)
        {
            if (this.eventHandlers.ContainsKey(eventName))
            {
                this.eventHandlers[eventName].Remove(handler);
            }
        }

        private void HandleLuaEvent(LuaEvent luaEvent)
        {
            if (this.eventHandlers.ContainsKey(luaEvent.Name))
            {
                var handlers = this.eventHandlers[luaEvent.Name];
                foreach (var handler in handlers)
                {
                    handler.Invoke(luaEvent);
                }
            }

            foreach (var pair in this.luaEventHandlers)
            {
                if(pair.Value.TryGetValue(luaEvent.Name, out var action))
                {
                    action.Invoke(luaEvent);
                }
            }
        }
    }
}
