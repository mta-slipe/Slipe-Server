using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Resources;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Services;

public interface ILuaEventService
{
    void AddEventHandler(string eventName, Action<LuaEvent> handler);
    void RemoveEventHandler(string eventName, Action<LuaEvent> handler);
    void TriggerEvent(string eventName, IElement? source = null);
    void TriggerEvent(string eventName, IElement? source = null, params LuaValue[] parameters);
    void TriggerEvent(string eventName, IElement? source = null, params object[] parameters);
    void TriggerEventFor(Player player, string eventName, IElement? source = null);
    void TriggerEventFor(Player player, string eventName, IElement? source = null, params LuaValue[] parameters);
    void TriggerEventFor(Player player, string eventName, IElement? source = null, params object[] parameters);
    void TriggerEventForMany(IEnumerable<Player> players, string eventName, IElement? source = null);
    void TriggerEventForMany(IEnumerable<Player> players, string eventName, IElement? source = null, params LuaValue[] parameters);
    void TriggerEventForMany(IEnumerable<Player> players, string eventName, IElement? source = null, params object[] parameters);
    void TriggerLatentEvent(string eventName, Resource sourceResource, IElement? source = null, int rate = 50000);
    void TriggerLatentEvent(string eventName, Resource sourceResource, IElement? source = null, int rate = 50000, params LuaValue[] parameters);
    void TriggerLatentEvent(string eventName, Resource sourceResource, IElement? source = null, int rate = 50000, params object[] parameters);
    void TriggerLatentEventFor(Player player, string eventName, Resource sourceResource, IElement? source = null, int rate = 50000);
    void TriggerLatentEventFor(Player player, string eventName, Resource sourceResource, IElement? source = null, int rate = 50000, params LuaValue[] parameters);
    void TriggerLatentEventFor(Player player, string eventName, Resource sourceResource, IElement? source = null, int rate = 50000, params object[] parameters);
    void TriggerLatentEventForMany(IEnumerable<Player> players, string eventName, Resource sourceResource, IElement? source = null, int rate = 50000);
    void TriggerLatentEventForMany(IEnumerable<Player> players, string eventName, Resource sourceResource, IElement? source = null, int rate = 50000, params LuaValue[] parameters);
    void TriggerLatentEventForMany(IEnumerable<Player> players, string eventName, Resource sourceResource, IElement? source = null, int rate = 50000, params object[] parameters);
}
