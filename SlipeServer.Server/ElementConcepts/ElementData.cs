using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Concepts;

[Obsolete("It is highly not recommended to use element data!")]
public interface ISupportsElementData
{
    ElementData ElementData { get; }
}

[Obsolete("It is highly not recommended to use element data!")]
public class ElementData
{
    public event ElementEventHandler<Element, ElementDataChangedArgs>? Changed;
    private Dictionary<string, ElementDataItem> Items { get; set; }
    /// <summary>
    /// Lists all the players that are subscribed to one (or more) element data entries on this element.
    /// </summary>
    public ConcurrentDictionary<Player, ConcurrentDictionary<string, bool>> ElementDataSubscriptions { get; set; }

    /// <summary>
    /// A read-only representation of all element data that is broadcastable
    /// </summary>
    public Packets.Definitions.Entities.Structs.CustomData BroadcastableElementData => new()
    {
        Items = this.Items
            .Where(x => x.Value.SyncType == DataSyncType.Broadcast)
            .Select(x => new Packets.Definitions.Entities.Structs.CustomDataItem()
            {
                Name = x.Key,
                Data = x.Value.Value
            })
    };

    public Element Element { get; }

    public ElementData(Element element)
    {
        this.Items = [];
        this.ElementDataSubscriptions = new();
        this.Element = element;
    }

    /// <summary>
    /// Sets element data on this element
    /// </summary>
    /// <param name="key">The key to store the value under</param>
    /// <param name="value">The value to store</param>
    /// <param name="syncType">The type of synchronisation to do with this value</param>
    public void SetData(string key, LuaValue value, DataSyncType syncType = DataSyncType.Local)
    {
        LuaValue? oldValue = this.GetData(key);

        if (value.IsNil)
            this.Items.Remove(key);
        else
            this.Items[key] = new ElementDataItem(key, value, syncType);

        this.Changed?.Invoke(Element, new ElementDataChangedArgs(key, value, oldValue, syncType));
    }

    /// <summary>
    /// Gets element data on this element if it exists
    /// </summary>
    /// <param name="dataName">The key to retrieve the stored value from</param>
    /// <param name="inherit">Whether the value should be attempted to be fetched from the element's parent(s) as well</param>
    /// <returns>The value if it exists, null otherwise.</returns>
    public LuaValue? GetData(string dataName, bool inherit = false)
    {
        if (this.Items.TryGetValue(dataName, out var value))
            return value.Value;

        if (inherit)
        {
            if(Element.Parent is ISupportsElementData parentWithCustomData)
            {
                return parentWithCustomData.ElementData.GetData(dataName, inherit);
            }
        }

        return null;
    }

    /// <summary>
    /// Subscribes a player to changes to a specific element data value
    /// </summary>
    /// <param name="player">The player to subscribe</param>
    /// <param name="key">The key of the data to subscribe to</param>
    public void SubscribeToData(Player player, string key)
    {
        if (!this.ElementDataSubscriptions.ContainsKey(player))
        {
            this.ElementDataSubscriptions[player] = new();
            player.Destroyed += HandleElementDataSubscriberDestruction;
        }
        this.ElementDataSubscriptions[player].TryAdd(key, true);

    }

    private void HandleElementDataSubscriberDestruction(Element element)
    {
        if (element is Player player)
            UnsubscribeFromAllData(player);
    }

    /// <summary>
    /// Unsubscribes a player from changes to a specific element data value
    /// </summary>
    /// <param name="player">The player to unsubscribe</param>
    /// <param name="key">The key of the data to unsubscribe from</param>
    public void UnsubscribeFromData(Player player, string key)
    {
        if (this.ElementDataSubscriptions.TryGetValue(player, out var keys))
        {
            keys.Remove(key, out var _);
            if (keys.IsEmpty)
                UnsubscribeFromAllData(player);
        }
    }

    /// <summary>
    /// Unsubscribes an element from all element data on this element
    /// </summary>
    /// <param name="player"></param>
    public void UnsubscribeFromAllData(Player player)
    {
        player.Destroyed -= HandleElementDataSubscriberDestruction;
        this.ElementDataSubscriptions.Remove(player, out var keys);
    }

    /// <summary>
    /// Checks whether a player is subscribes to a specific data key on this element
    /// </summary>
    /// <param name="player">The player to check</param>
    /// <param name="key">The key for the element data to check</param>
    /// <returns></returns>
    public bool IsPlayerSubscribedToData(Player player, string key)
    {
        if (this.ElementDataSubscriptions.TryGetValue(player, out var keys))
            return keys.ContainsKey(key);

        return false;
    }

    /// <summary>
    /// Returns all players subscribes to a specific element data key on this element
    /// </summary>
    /// <param name="key">The element data key</param>
    public IEnumerable<Player> GetPlayersSubcribedToData(string key)
    {
        return this.ElementDataSubscriptions.Keys
            .Where(x => this.ElementDataSubscriptions[x].ContainsKey(key));
    }

}
