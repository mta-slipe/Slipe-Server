using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using System.Numerics;

namespace SlipeServer.Server.Elements;

/// <summary>
/// World object element
/// World objects are physical representations of any object in the world.
/// </summary>
public class WorldObject : Element
{
    public override ElementType ElementType => ElementType.Object;

    protected ushort model;
    public ushort Model
    {
        get => this.model;
        set
        {
            if (this.model == value)
                return;

            var args = new ElementChangedEventArgs<WorldObject, ushort>(this, this.Model, value, this.IsSync);
            this.model = value;
            ModelChanged?.Invoke(this, args);
        }
    }

    public bool IsLowLod { get; set; } = false;
    public WorldObject? LowLodElement { get; set; }
    public bool DoubleSided { get; set; } = false;
    public PositionRotationAnimation? Movement { get; set; }

    protected Vector3 scale = Vector3.One;
    public Vector3 Scale
    {
        get => this.scale;
        set
        {
            if (this.scale == value)
                return;

            var args = new ElementChangedEventArgs<WorldObject, Vector3>(this, this.Scale, value, this.IsSync);
            this.scale = value;
            ScaleChanged?.Invoke(this, args);
        }
    }

    public float Health { get; set; } = 1000;

    private bool isVisibleInAllDimensions;
    public bool IsVisibleInAllDimensions
    {
        get => this.isVisibleInAllDimensions;
        set
        {
            if (this.isVisibleInAllDimensions == value)
                return;

            var args = new ElementChangedEventArgs<WorldObject, bool>(this, this.isVisibleInAllDimensions, value, this.IsSync);
            this.isVisibleInAllDimensions = value;
            IsVisibleInAllDimensionsChanged?.Invoke(this, args);
        }
    }

    public WorldObject(ObjectModel model, Vector3 position)
    {
        this.Model = (ushort)model;
        this.Position = position;
    }

    public WorldObject(ushort model, Vector3 position)
    {
        this.Model = model;
        this.Position = position;
    }

    public new WorldObject AssociateWith(MtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public event ElementChangedEventHandler<WorldObject, ushort>? ModelChanged;
    public event ElementChangedEventHandler<WorldObject, Vector3>? ScaleChanged;
    public event ElementChangedEventHandler<WorldObject, bool>? IsVisibleInAllDimensionsChanged;
}
