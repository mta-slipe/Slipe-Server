using SlipeServer.Server.Elements.ColShapes;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Elements.Events;
using System.Drawing;
using System.Numerics;
namespace SlipeServer.Server.Elements;

/// <summary>
/// A marker element
/// A marker represent a visual marking in the game. This can be in one of several shapes based on the marker type.
/// </summary>
public class Marker : Element
{
    public override ElementType ElementType => ElementType.Marker;


    private IMtaServer? associatedServer;
    public CollisionShape? ColShape { get; private set; }

    private MarkerType markerType;
    public MarkerType MarkerType
    {
        get => this.markerType;
        set
        {
            if (this.markerType == value)
                return;

            var args = new ElementChangedEventArgs<Marker, MarkerType>(this, this.markerType, value, this.IsSync);
            this.markerType = value;
            MarkerTypeChanged?.Invoke(this, args);
            if (this.ColShape != null)
                RecreateCollisionShape();
        }
    }

    private MarkerIcon markerIcon;
    public MarkerIcon MarkerIcon
    {
        get => this.markerIcon;
        set
        {
            if (this.markerIcon == value)
                return;

            var args = new ElementChangedEventArgs<Marker, MarkerIcon>(this, this.markerIcon, value, this.IsSync);
            this.markerIcon = value;
            MarkerIconChanged?.Invoke(this, args);
        }
    }

    private float size = 1;
    public float Size
    {
        get => this.size;
        set
        {
            if (this.size == value)
                return;

            var args = new ElementChangedEventArgs<Marker, float>(this, this.size, value, this.IsSync);
            this.size = value;
            SizeChanged?.Invoke(this, args);

            if (this.ColShape is CollisionCircle circle)
                circle.Radius = value;
            else if (this.ColShape is CollisionTube tube)
            {
                tube.Radius = value;
                tube.Height = value <= 1.5f ? value + 1f : value;
            }
            else if (this.ColShape is CollisionSphere sphere)
                sphere.Radius = value;
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

            var args = new ElementChangedEventArgs<Marker, Color>(this, this.color, value, this.IsSync);
            this.color = value;
            ColorChanged?.Invoke(this, args);
        }
    }

    private Vector3? targetPosition;
    public Vector3? TargetPosition
    {
        get => this.targetPosition;
        set
        {
            if (this.targetPosition == value)
                return;

            var args = new ElementChangedEventArgs<Marker, Vector3?>(this, this.targetPosition, value, this.IsSync);
            this.targetPosition = value;
            TargetPositionChanged?.Invoke(this, args);
        }
    }

    public Color? TargetArrowColor
    {
        get => field;
        set
        {
            if (field == value)
                return;

            var args = new ElementChangedEventArgs<Marker, Color?>(this, field, value, this.IsSync);
            field = value;
            TargetArrowColorChanged?.Invoke(this, args);
        }
    }

    public float TargetArrowSize
    {
        get => field;
        set
        {
            if (field == value)
                return;
            var args = new ElementChangedEventArgs<Marker, float?>(this, field, value, this.IsSync);
            field = value;
            TargetArrowSizeChanged?.Invoke(this, args);
        }
    }

    public bool IgnoreAlphaLimits
    {
        get => field;
        set
        {
            if (field == value)
                return;
            var args = new ElementChangedEventArgs<Marker, bool>(this, field, value, this.IsSync);
            field = value;
            IgnoreAlphaLimitsChanged?.Invoke(this, args);
        }
    }

    public Marker(Vector3 position, MarkerType markerType, bool withCollisionShape = true)
    {
        this.Position = position;
        this.MarkerType = markerType;
        this.PositionChanged += (_, args) =>
        {
            if (this.ColShape is CollisionCircle circle)
                circle.Position2 = new Vector2(args.NewValue.X, args.NewValue.Y);
            else if (this.ColShape != null)
                this.ColShape.Position = args.NewValue;
        };
        if (withCollisionShape)
            this.ColShape = CreateCollisionShape();
    }

    public override Marker AssociateWith(IMtaServer server)
    {
        this.associatedServer = server;
        base.AssociateWith(server);
        this.ColShape?.AssociateWith(server);
        return this;
    }

    public void RecreateCollisionShape()
    {
        if (this.ColShape != null)
        {
            this.ColShape.ElementEntered -= HandleColShapeEntered;
            this.ColShape.ElementLeft -= HandleColShapeLeft;
            this.ColShape.Destroy();
        }
        this.ColShape = CreateCollisionShape();
        if (this.associatedServer != null)
            this.ColShape.AssociateWith(this.associatedServer);
    }

    private CollisionShape CreateCollisionShape()
    {
        CollisionShape colShape = this.markerType switch
        {
            MarkerType.Checkpoint => new CollisionCircle(new Vector2(this.Position.X, this.Position.Y), this.size),
            MarkerType.Cylinder => new CollisionTube(this.Position, this.size, this.size <= 1.5f ? this.size + 1f : this.size),
            _ => new CollisionSphere(this.Position, this.size),
        };
        colShape.ElementEntered += HandleColShapeEntered;
        colShape.ElementLeft += HandleColShapeLeft;
        return colShape;
    }

    private void HandleColShapeEntered(CollisionShape sender, CollisionShapeHitEventArgs e)
        => this.ElementHit?.Invoke(this, new MarkerHitEventArgs(e.Element, e.Element.Dimension == this.Dimension));

    private void HandleColShapeLeft(CollisionShape sender, CollisionShapeLeftEventArgs e)
        => this.ElementLeft?.Invoke(this, new MarkerLeftEventArgs(e.Element, e.Element.Dimension == this.Dimension));

    public event ElementChangedEventHandler<Marker, MarkerType>? MarkerTypeChanged;
    public event ElementChangedEventHandler<Marker, MarkerIcon>? MarkerIconChanged;
    public event ElementChangedEventHandler<Marker, float>? SizeChanged;
    public event ElementChangedEventHandler<Marker, Color>? ColorChanged;
    public event ElementChangedEventHandler<Marker, Vector3?>? TargetPositionChanged;
    public event ElementChangedEventHandler<Marker, Color?>? TargetArrowColorChanged;
    public event ElementChangedEventHandler<Marker, float?>? TargetArrowSizeChanged;
    public event ElementChangedEventHandler<Marker, bool>? IgnoreAlphaLimitsChanged;

    public event ElementEventHandler<Marker, MarkerHitEventArgs>? ElementHit;
    public event ElementEventHandler<Marker, MarkerLeftEventArgs>? ElementLeft;
}
