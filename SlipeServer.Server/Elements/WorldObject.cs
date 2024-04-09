using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using System;
using System.Numerics;
using System.Threading.Tasks;

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
    public bool IsBreakable { get; set; } = false;

    private readonly object movementLock = new();
    public PositionRotationAnimation? Movement { get; private set; }

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

    public Vector3 ApproximatedMidMovementPosition
    {
        get
        {
            if (this.Movement == null)
                return this.Position;

            return this.Position + (this.Movement.DeltaPosition * (float)this.Movement.Progress);
        }
    }

    public Vector3 ApproximatedMidMovementRotation
    {
        get
        {
            if (this.Movement == null)
                return this.Rotation;

            return this.Rotation + (this.Movement.DeltaRotation * (float)this.Movement.Progress);
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

    public Task Move(Vector3 newPosition, Vector3 deltaRotation, TimeSpan moveTime)
    {
        PositionRotationAnimation movement;
        lock (this.movementLock)
        {
            movement = new PositionRotationAnimation()
            {
                SourcePosition = this.position,
                SourceRotation = this.rotation,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow + moveTime,

                EasingType = "linear",
                TargetPosition = newPosition,
                TargetRotation = this.rotation + deltaRotation,

                DeltaRotation = deltaRotation,
                DeltaRotationMode = true
            };

            this.Movement = movement;
        }

        this.Moved?.Invoke(this, new (this.Movement));

        var task = Task.Delay(moveTime);

        AwaitMovementAndUpdatePosition(task, movement);

        return task;
    }

    public void CancelMovement(bool resetPosition = false)
    {
        lock (this.movementLock)
        {
            if (this.Movement == null)
                return;

            if (!resetPosition)
            {
                this.position = this.ApproximatedMidMovementPosition;
                this.rotation = this.ApproximatedMidMovementRotation;
            }

            this.MovementCancelled?.Invoke(this, new (this.position, this.rotation));
            this.Movement = null;
        }
    }

    private async void AwaitMovementAndUpdatePosition(Task task, PositionRotationAnimation movement)
    {
        await task;
        lock (this.movementLock)
        {
            if (this.Movement != movement)
                return;

            this.position = this.Movement.TargetPosition;
            this.rotation = this.Movement.TargetRotation;
            this.Movement = null;
        }
    }

    public event ElementChangedEventHandler<WorldObject, ushort>? ModelChanged;
    public event ElementChangedEventHandler<WorldObject, Vector3>? ScaleChanged;
    public event ElementChangedEventHandler<WorldObject, bool>? IsVisibleInAllDimensionsChanged;
    public event ElementEventHandler<WorldObject, WorldObjectMovedEventArgs>? Moved;
    public event ElementEventHandler<WorldObject, WorldObjectMovementCancelledEventArgs>? MovementCancelled;
}
