using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Lua.Event;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Extensions;
using SlipeServer.Server.Mappers;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Services;

public class LuaEventService
{
    private readonly MtaServer server;
    private readonly RootElement root;
    private readonly LatentPacketService latentPacketService;
    private readonly IElementCollection elementRepository;
    private readonly LuaValueMapper mapper;
    private readonly Dictionary<string, List<Action<LuaEvent>>> eventHandlers;

    public LuaEventService(MtaServer server,
        RootElement root,
        LatentPacketService latentPacketService,
        IElementCollection elementRepository,
        LuaValueMapper mapper)
    {
        this.server = server;
        this.root = root;
        this.latentPacketService = latentPacketService;
        this.elementRepository = elementRepository;
        this.mapper = mapper;
        this.eventHandlers = new Dictionary<string, List<Action<LuaEvent>>>();

        server.LuaEventTriggered += HandleLuaEvent;
    }

    public void TriggerEvent(string eventName, Element? source = null, params LuaValue[] parameters)
    {
        this.server.BroadcastPacket(new LuaEventPacket(eventName, (source ?? this.root).Id, parameters));
    }

    public void TriggerEvent(string eventName, Element? source = null, params object[] parameters)
    {
        TriggerEvent(eventName, source, parameters.Select(x => this.mapper.Map(x)).ToArray());
    }

    public void TriggerEvent(string eventName, Element? source = null)
    {
        TriggerEvent(eventName, source, Array.Empty<LuaValue>());
    }


    public void TriggerEventFor(Player player, string eventName, Element? source = null, params LuaValue[] parameters)
    {
        new LuaEventPacket(eventName, (source ?? this.root).Id, parameters).SendTo(player);
    }

    public void TriggerEventFor(Player player, string eventName, Element? source = null, params object[] parameters)
    {
        TriggerEventFor(player, eventName, source, parameters.Select(x => this.mapper.Map(x)).ToArray());
    }

    public void TriggerEventFor(Player player, string eventName, Element? source = null)
    {
        TriggerEventFor(player, eventName, source, Array.Empty<LuaValue>());
    }


    public void TriggerEventForMany(IEnumerable<Player> players, string eventName, Element? source = null, params LuaValue[] parameters)
    {
        new LuaEventPacket(eventName, (source ?? this.root).Id, parameters).SendTo(players);
    }

    public void TriggerEventForMany(IEnumerable<Player> players, string eventName, Element? source = null, params object[] parameters)
    {
        TriggerEventForMany(players, eventName, source, parameters.Select(x => this.mapper.Map(x)).ToArray());
    }

    public void TriggerEventForMany(IEnumerable<Player> players, string eventName, Element? source = null)
    {
        TriggerEventForMany(players, eventName, source, Array.Empty<LuaValue>());
    }


    public void TriggerLatentEvent(string eventName, Resource sourceResource, Element? source = null, int rate = 50000, params LuaValue[] parameters)
    {
        TriggerLatentEventForMany(this.elementRepository.GetByType<Player>(ElementType.Player), eventName, sourceResource, source, rate, parameters);
    }

    public void TriggerLatentEvent(string eventName, Resource sourceResource, Element? source = null, int rate = 50000, params object[] parameters)
    {
        TriggerLatentEvent(eventName, sourceResource, source, rate, parameters.Select(x => this.mapper.Map(x)).ToArray());
    }

    public void TriggerLatentEvent(string eventName, Resource sourceResource, Element? source = null, int rate = 50000)
    {
        TriggerLatentEvent(eventName, sourceResource, source, rate, Array.Empty<LuaValue>());
    }


    public void TriggerLatentEventFor(Player player, string eventName, Resource sourceResource, Element? source = null, int rate = 50000, params LuaValue[] parameters)
    {
        var packet = new LuaEventPacket(eventName, (source ?? this.root).Id, parameters);
        this.latentPacketService.EnqueueLatentPacket(new Player[] { player }, packet, sourceResource.NetId, rate);
    }

    public void TriggerLatentEventFor(Player player, string eventName, Resource sourceResource, Element? source = null, int rate = 50000, params object[] parameters)
    {
        TriggerLatentEventFor(player, eventName, sourceResource, source, rate, parameters.Select(x => this.mapper.Map(x)).ToArray());
    }

    public void TriggerLatentEventFor(Player player, string eventName, Resource sourceResource, Element? source = null, int rate = 50000)
    {
        TriggerLatentEventFor(player, eventName, sourceResource, source, rate, Array.Empty<LuaValue>());
    }


    public void TriggerLatentEventForMany(IEnumerable<Player> players, string eventName, Resource sourceResource, Element? source = null, int rate = 50000, params LuaValue[] parameters)
    {
        var packet = new LuaEventPacket(eventName, (source ?? this.root).Id, parameters);
        this.latentPacketService.EnqueueLatentPacket(players, packet, sourceResource.NetId, rate);
    }

    public void TriggerLatentEventForMany(IEnumerable<Player> players, string eventName, Resource sourceResource, Element? source = null, int rate = 50000, params object[] parameters)
    {
        TriggerLatentEventForMany(players, eventName, sourceResource, source, rate, parameters.Select(x => this.mapper.Map(x)).ToArray());
    }

    public void TriggerLatentEventForMany(IEnumerable<Player> players, string eventName, Resource sourceResource, Element? source = null, int rate = 50000)
    {
        TriggerLatentEventForMany(players, eventName, sourceResource, source, rate, Array.Empty<LuaValue>());
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
