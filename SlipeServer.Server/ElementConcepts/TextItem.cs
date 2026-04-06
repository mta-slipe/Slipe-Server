using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Server.Concepts;

/// <summary>
/// Represents a 2d text item displayed on the client's screen.
/// </summary>
public class TextItem
{
    private readonly ITextItemService service;
    private readonly List<TextDisplay> displays = [];

    private string text;
    private float x;
    private float y;
    private int priority;
    private Color color;
    private float scale;
    private HorizontalTextAlignment alignX;
    private VerticalTextAlignment alignY;
    private byte shadowAlpha;

    public ulong Id { get; }

    public string Text
    {
        get => this.text;
        set { this.text = value; SendUpdateToObservers(); }
    }

    public float X
    {
        get => this.x;
        set { this.x = value; SendUpdateToObservers(); }
    }

    public float Y
    {
        get => this.y;
        set { this.y = value; SendUpdateToObservers(); }
    }

    public int Priority
    {
        get => this.priority;
        set => this.priority = value;
    }

    public Color Color
    {
        get => this.color;
        set { this.color = value; SendUpdateToObservers(); }
    }

    public float Scale
    {
        get => this.scale;
        set { this.scale = value; SendUpdateToObservers(); }
    }

    public HorizontalTextAlignment AlignX
    {
        get => this.alignX;
        set { this.alignX = value; SendUpdateToObservers(); }
    }

    public VerticalTextAlignment AlignY
    {
        get => this.alignY;
        set { this.alignY = value; SendUpdateToObservers(); }
    }

    public byte ShadowAlpha
    {
        get => this.shadowAlpha;
        set { this.shadowAlpha = value; SendUpdateToObservers(); }
    }

    internal Vector2 Position => new Vector2(this.x, this.y);
    internal byte Format => (byte)(((byte)this.alignX) | ((byte)this.alignY));
    public IReadOnlyList<TextDisplay> Displays => this.displays.AsReadOnly();

    internal TextItem(
        ITextItemService service,
        ulong id,
        string text,
        float x,
        float y,
        int priority = 1,
        Color? color = null,
        float scale = 1,
        HorizontalTextAlignment alignX = HorizontalTextAlignment.Left,
        VerticalTextAlignment alignY = VerticalTextAlignment.Top,
        byte shadowAlpha = 0)
    {
        this.service = service;
        this.Id = id;
        this.text = text;
        this.x = x;
        this.y = y;
        this.priority = priority;
        this.color = color ?? Color.White;
        this.scale = scale;
        this.alignX = alignX;
        this.alignY = alignY;
        this.shadowAlpha = shadowAlpha;
    }

    internal void AddDisplay(TextDisplay display)
    {
        if (!this.displays.Contains(display))
            this.displays.Add(display);
    }

    internal void RemoveDisplay(TextDisplay display)
    {
        this.displays.Remove(display);
    }

    internal void SendTo(IEnumerable<Player> players)
    {
        this.service.SendTextItemTo(players, this);
    }

    internal void DeleteFrom(IEnumerable<Player> players)
    {
        this.service.DeleteTextItemFor(players, this);
    }

    private void SendUpdateToObservers()
    {
        var observers = this.displays
            .SelectMany(d => d.Observers)
            .Distinct()
            .ToList();

        if (observers.Count > 0)
            SendTo(observers);
    }
}
