using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// A text display is a canvas that can contain many text items and be observed by multiple players.
/// </summary>
public class TextDisplay
{
    private readonly List<Player> observers = [];
    private readonly List<TextItem> textItems = [];

    public IReadOnlyCollection<Player> Observers => this.observers.AsReadOnly();
    public IReadOnlyCollection<TextItem> TextItems => this.textItems.AsReadOnly();

    internal TextDisplay() { }

    public void AddObserver(Player player)
    {
        if (this.observers.Contains(player))
            return;

        this.observers.Add(player);
        player.Disconnected += HandlePlayerDisconnected;

        foreach (var item in this.textItems)
            item.SendTo([player]);
    }

    public void RemoveObserver(Player player)
    {
        player.Disconnected -= HandlePlayerDisconnected;

        if (!this.observers.Remove(player))
            return;

        foreach (var item in this.textItems)
            item.DeleteFrom([player]);
    }

    public bool IsObserver(Player player) => this.observers.Contains(player);

    public void AddText(TextItem item)
    {
        if (this.textItems.Contains(item))
            return;

        this.textItems.Add(item);
        item.AddDisplay(this);

        item.SendTo(this.observers);
    }

    public void RemoveText(TextItem item)
    {
        if (!this.textItems.Remove(item))
            return;

        item.RemoveDisplay(this);
        item.DeleteFrom(this.observers);
    }

    public void Destroy()
    {
        foreach (var item in this.textItems.ToList())
            RemoveText(item);

        foreach (var observer in this.observers)
            observer.Disconnected -= HandlePlayerDisconnected;

        this.observers.Clear();
    }

    private void HandlePlayerDisconnected(Player player, PlayerQuitEventArgs args)
    {
        RemoveObserver(player);
    }
}
