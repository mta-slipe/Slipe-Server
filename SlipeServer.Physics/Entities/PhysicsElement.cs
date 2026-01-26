using BepuPhysics;
using SlipeServer.Physics.Worlds;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using System.Numerics;

namespace SlipeServer.Physics.Entities;

public class PhysicsElement<TDescription, THandle>(THandle handle, TDescription description, PhysicsWorld physicsWorld, Simulation simulation)
{
    internal THandle handle = handle;
    internal TDescription description = description;
    protected readonly PhysicsWorld physicsWorld = physicsWorld;
    protected readonly Simulation simulation = simulation;

    protected virtual Vector3 Position { get; set; }
    protected virtual Quaternion Rotation { get; set; }

    protected Vector3 positionOffset;
    protected Vector3 rotationOffset;

    protected object positionUpdateLock = new();

    public Element? CoupledElement { get; private set; }

    public void CoupleWith(Element element, Vector3? positionOffset = null, Vector3? rotationOffset = null)
    {
        if (this.CoupledElement != null)
        {
            Decouple();
        }
        this.CoupledElement = element;

        element.PositionChanged += HandleCoupledElementPositionUpdate;
        element.RotationChanged += HandleCoupledElementRotationUpdate;

        this.positionOffset = positionOffset ?? Vector3.Zero;
        this.rotationOffset = rotationOffset ?? Vector3.Zero;

        this.Position = element.Position + this.positionOffset;
        this.Rotation = (element.Rotation + this.rotationOffset).ToQuaternion();
    }

    public void Decouple()
    {
        if (this.CoupledElement != null)
        {
            this.CoupledElement.PositionChanged -= HandleCoupledElementPositionUpdate;
            this.CoupledElement.RotationChanged -= HandleCoupledElementRotationUpdate;
        }
        this.CoupledElement = null;
    }

    protected virtual void HandleCoupledElementPositionUpdate(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        this.Position = args.NewValue + this.positionOffset;
    }

    protected virtual void HandleCoupledElementRotationUpdate(Element sender, ElementChangedEventArgs<Vector3> args)
    {
        this.Rotation = this.rotationOffset.ToQuaternion() * args.NewValue.ToQuaternion();
    }
}
