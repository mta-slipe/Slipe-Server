using SlipeServer.Server.Elements.Events;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Server.Elements;

/// <summary>
/// A radar area element
/// Radar areas are visual rectangles on the F11 map and ingame radar.
/// </summary>
public class RadarArea : Element
{
    public override ElementType ElementType => ElementType.RadarArea;

    public Vector2 Position2
    {
        get => new(this.Position.X, this.Position.Y);
        set => this.Position = new Vector3(value.X, value.Y, 0);
    }

    private Vector2 size;
    public Vector2 Size
    {
        get => this.size;
        set
        {
            if (this.size == value)
                return;

            var args = new ElementChangedEventArgs<RadarArea, Vector2>(this, this.size, value, this.IsSync);
            this.size = value;
            SizeChanged?.Invoke(this, args);
        }
    }

    private Color color;
    public Color Color
    {
        get => this.color;
        set
        {
            if (this.color == value)
                return;

            var args = new ElementChangedEventArgs<RadarArea, Color>(this, this.color, value, this.IsSync);
            this.color = value;
            ColorChanged?.Invoke(this, args);
        }
    }

    private bool isFlashing = false;
    public bool IsFlashing
    {
        get => this.isFlashing;
        set
        {
            if (this.isFlashing == value)
                return;

            var args = new ElementChangedEventArgs<RadarArea, bool>(this, this.isFlashing, value, this.IsSync);
            this.isFlashing = value;
            FlashingStateChanged?.Invoke(this, args);
        }
    }

    public RadarArea(Vector2 position, Vector2 size, Color color)
    {
        this.Position2 = position;
        this.Size = size;
        this.Color = color;
    }

    private float[] MinMax(float left, float right)
    {
        if (right < left)
        {
            return [right, left];
        }
        return [left, right];
    }

    public bool IsInside(Vector2 position)
    {
        float[] horizontalBounds = MinMax(this.Position2.X, this.Position2.X + this.Size.X);
        float[] verticalBounds = MinMax(this.Position2.Y, this.Position2.Y + this.Size.Y);

        if (position.X >= horizontalBounds[0] && position.X <= horizontalBounds[1])
            if (position.Y >= verticalBounds[0] && position.Y <= verticalBounds[1])
            {
                return true;
            }
        return false;
    }

    public new RadarArea AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public event ElementChangedEventHandler<RadarArea, Vector2>? SizeChanged;
    public event ElementChangedEventHandler<RadarArea, Color>? ColorChanged;
    public event ElementChangedEventHandler<RadarArea, bool>? FlashingStateChanged;
}
