using SlipeServer.Packets.Definitions.Entities.Structs;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Enums;
using System;
using System.Numerics;
using System.Threading;
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

    public bool IsLowLod { get; init; } = false;

    private WorldObject? lowLodElement;
    public WorldObject? LowLodElement
    {
        get => this.lowLodElement;
        set
        {
            if (this.lowLodElement == value)
                return;

            var args = new ElementChangedEventArgs<WorldObject, WorldObject?>(this, this.lowLodElement, value, this.IsSync);
            this.lowLodElement = value;
            LowLodElementChanged?.Invoke(this, args);
        }
    }

    private bool doubleSided;
    public bool DoubleSided
    {
        get => this.doubleSided;
        set
        {
            if (this.doubleSided == value)
                return;

            var args = new ElementChangedEventArgs<WorldObject, bool>(this, this.doubleSided, value, this.IsSync);
            this.doubleSided = value;
            DoubleSidedChanged?.Invoke(this, args);
        }
    }

    private bool isBreakable;
    public bool IsBreakable
    {
        get => this.isBreakable;
        set
        {
            if (this.isBreakable == value)
                return;

            var args = new ElementChangedEventArgs<WorldObject, bool>(this, this.isBreakable, value, this.IsSync);
            this.isBreakable = value;
            IsBreakableChanged?.Invoke(this, args);
        }
    }

    private bool isBroken;
    public bool IsBroken
    {
        get => this.isBroken;
        set
        {
            if (this.isBroken == value)
                return;

            var args = new ElementChangedEventArgs<WorldObject, bool>(this, this.isBroken, value, this.IsSync);
            this.isBroken = value;
            IsBrokenChanged?.Invoke(this, args);
        }
    }

    private bool isRespawnable;
    public bool IsRespawnable
    {
        get => this.isRespawnable;
        set
        {
            if (this.isRespawnable == value)
                return;

            var args = new ElementChangedEventArgs<WorldObject, bool>(this, this.isRespawnable, value, this.IsSync);
            this.isRespawnable = value;
            IsRespawnableChanged?.Invoke(this, args);
        }
    }

    private readonly Lock movementLock = new();
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

    public new WorldObject AssociateWith(IMtaServer server)
    {
        base.AssociateWith(server);
        return this;
    }

    public Task Move(Vector3 newPosition, Vector3 deltaRotation, TimeSpan moveTime)
    {
        PositionRotationAnimation movement;
        lock (this.movementLock)
        {
            if (this.Movement != null)
                CancelMovement(false, false);

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

        this.Moved?.Invoke(this, new (movement));

        var task = Task.Delay(moveTime);

        AwaitMovementAndUpdatePosition(task, movement);

        return task;
    }

    public void CancelMovement(bool resetPosition = false, bool raiseEvents = true)
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

            if (raiseEvents)
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
    public event ElementChangedEventHandler<WorldObject, WorldObject?>? LowLodElementChanged;
    public event ElementChangedEventHandler<WorldObject, bool>? DoubleSidedChanged;
    public event ElementChangedEventHandler<WorldObject, bool>? IsBreakableChanged;
    public event ElementChangedEventHandler<WorldObject, bool>? IsBrokenChanged;
    public event ElementChangedEventHandler<WorldObject, bool>? IsRespawnableChanged;
    public event ElementChangedEventHandler<WorldObject, bool>? IsVisibleInAllDimensionsChanged;
    public event ElementEventHandler<WorldObject, WorldObjectMovedEventArgs>? Moved;
    public event ElementEventHandler<WorldObject, WorldObjectMovementCancelledEventArgs>? MovementCancelled;
}
