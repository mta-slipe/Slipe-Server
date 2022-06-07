using SlipeServer.Server.Elements.Events;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Server.Elements;

public class Blip : Element
{
    public override ElementType ElementType => ElementType.Blip;

    private short ordering;
    public short Ordering
    {
        get => this.ordering;
        set
        {
            var args = new ElementChangedEventArgs<Blip, short>(this, this.ordering, value, this.IsSync);
            this.ordering = value;
            this.OrderingChanged?.Invoke(this, args);
        }
    }

    private ushort visibleDistance;
    public ushort VisibleDistance
    {
        get => this.visibleDistance;
        set
        {
            var args = new ElementChangedEventArgs<Blip, ushort>(this, this.visibleDistance, value, this.IsSync);
            this.visibleDistance = value;
            this.VisibleDistanceChanged?.Invoke(this, args);
        }
    }

    private BlipIcon icon;
    public BlipIcon Icon
    {
        get => this.icon;
        set
        {
            var args = new ElementChangedEventArgs<Blip, BlipIcon>(this, this.icon, value, this.IsSync);
            this.icon = value;
            this.IconChanged?.Invoke(this, args);
        }
    }

    private byte size = 1;
    public byte Size
    {
        get => this.size;
        set
        {
            var args = new ElementChangedEventArgs<Blip, byte>(this, this.size, value, this.IsSync);
            this.size = value;
            this.SizeChanged?.Invoke(this, args);
        }
    }


    private Color color = Color.White;
    public Color Color
    {
        get => this.color;
        set
        {
            var args = new ElementChangedEventArgs<Blip, Color>(this, this.color, value, this.IsSync);
            this.color = value;
            this.ColorChanged?.Invoke(this, args);
        }
    }

    public Blip(Vector3 position, BlipIcon icon, ushort visibleDistance = 0, short ordering = 0)
    {
        this.Position = position;
        this.Icon = icon;
        this.VisibleDistance = visibleDistance;
        this.Ordering = ordering;
    }

    public new Blip AssociateWith(MtaServer server)
    {
        return server.AssociateElement(this);
    }

    public event ElementChangedEventHandler<Blip, short>? OrderingChanged;
    public event ElementChangedEventHandler<Blip, ushort>? VisibleDistanceChanged;
    public event ElementChangedEventHandler<Blip, BlipIcon>? IconChanged;
    public event ElementChangedEventHandler<Blip, byte>? SizeChanged;
    public event ElementChangedEventHandler<Blip, Color>? ColorChanged;
}
