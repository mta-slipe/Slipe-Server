using SlipeServer.Packets.Definitions.Entities.Structs;
using System;
using System.Numerics;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Elements.Events;

namespace SlipeServer.Server.Elements;

public class WorldObject : Element
{
    public override ElementType ElementType => ElementType.Object;

    protected ushort model;
    public ushort Model
    {
        get => this.model;
        set
        {
            var args = new ElementChangedEventArgs<WorldObject, ushort>(this, this.Model, value, this.IsSync);
            this.model = value;
            ModelChanged?.Invoke(this, args);
        }
    }

    public bool IsLowLod { get; set; } = false;
    public WorldObject? LowLodElement { get; set; }
    public bool DoubleSided { get; set; } = false;
    public bool IsVisibleInAllDimensions { get; set; } = true;
    public PositionRotationAnimation? Movement { get; set; }

    protected Vector3 scale = Vector3.One;
    public Vector3 Scale
    {
        get => this.scale;
        set
        {
            var args = new ElementChangedEventArgs<WorldObject, Vector3>(this, this.Scale, value, this.IsSync);
            this.scale = value;
            ScaleChanged?.Invoke(this, args);
        }
    }

    public bool IsFrozen { get; set; } = false;
    public float Health { get; set; } = 1000;

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
        return server.AssociateElement(this);
    }

    public event ElementChangedEventHandler<WorldObject, ushort>? ModelChanged;
    public event ElementChangedEventHandler<WorldObject, Vector3>? ScaleChanged;
}
