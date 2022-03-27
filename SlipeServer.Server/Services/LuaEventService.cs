using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Extensions;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Services;

public class LuaEventService
{
    private readonly MtaServer server;
    private readonly RootElement root;

    private readonly Dictionary<string, List<Action<LuaEvent>>> eventHandlers;

    public LuaEventService(MtaServer server, RootElement root)
    {
        this.server = server;
        this.root = root;

        this.eventHandlers = new Dictionary<string, List<Action<LuaEvent>>>();

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
    }
}
