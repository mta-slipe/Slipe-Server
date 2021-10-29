using BepuPhysics;
using SlipeServer.Physics.Worlds;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Elements.Events;
using SlipeServer.Server.Extensions;
using System.Numerics;

namespace SlipeServer.Physics.Entities
{
    public class PhysicsElement<TDescription, THandle>
    {
        internal THandle handle;
        internal TDescription description;
        protected readonly PhysicsWorld physicsWorld;
        protected readonly Simulation simulation;

        protected virtual Vector3 Position { get; set; }
        protected virtual Quaternion Rotation { get; set; }

        protected Vector3 positionOffset;
        protected Vector3 rotationOffset;

        protected object positionUpdateLock = new();

        public Element? CoupledElement { get; private set; }

        public PhysicsElement(THandle handle, TDescription description, PhysicsWorld physicsWorld, Simulation simulation)
        {
            this.handle = handle;
            this.description = description;
            this.physicsWorld = physicsWorld;
            this.simulation = simulation;
        }

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
            this.Rotation = (args.NewValue + this.rotationOffset).ToQuaternion();
        }
    }

    public class StaticPhysicsElement : PhysicsElement<StaticDescription, StaticHandle>
    {
        protected override Vector3 Position
        {
            get => this.description.Pose.Position;
            set
            {
                this.description.Pose.Position = value;
                lock (this.positionUpdateLock)
                    lock (this.physicsWorld.stepLock)
                        this.simulation.Statics.ApplyDescription(this.handle, this.description);
            }
        }

        protected override Quaternion Rotation
        {
            get => this.description.Pose.Orientation;
            set
            {
                this.description.Pose.Orientation = value;
                lock (this.positionUpdateLock)
                    lock (this.physicsWorld.stepLock)
                        this.simulation.Statics.ApplyDescription(this.handle, this.description);
            }
        }

        public StaticPhysicsElement(StaticHandle handle, StaticDescription description, PhysicsWorld physicsWorld, Simulation simulation) : 
            base(handle, description, physicsWorld, simulation)
        {
        }
    }

    public class DynamicBodyPhysicsElement : PhysicsElement<BodyDescription, BodyHandle>
    {
        public DynamicBodyPhysicsElement(BodyHandle handle, BodyDescription description, PhysicsWorld physicsWorld, Simulation simulation) :
            base(handle, description, physicsWorld, simulation)
        {
            physicsWorld.Stepped += HandlePhysicsWorldStep;
        }

        private void HandlePhysicsWorldStep()
        {
            if (this.CoupledElement != null)
            {
                var pose = this.simulation.Bodies.GetBodyReference(this.handle).Pose;
                this.CoupledElement.Position = pose.Position;
                this.CoupledElement.Rotation = pose.Orientation.ToEuler();
            }
        }
    }

    public class KinematicBodyPhysicsElement : PhysicsElement<BodyDescription, BodyHandle>
    {
        protected override Vector3 Position
        {
            get => this.description.Pose.Position;
            set
            {
                this.description.Pose.Position = value;
                lock (this.positionUpdateLock)
                    lock (this.physicsWorld.stepLock)
                        this.simulation.Bodies.ApplyDescription(this.handle, this.description);
            }
        }

        protected override Quaternion Rotation
        {
            get => this.description.Pose.Orientation;
            set
            {
                this.description.Pose.Orientation = value;
                lock (this.positionUpdateLock)
                    lock (this.physicsWorld.stepLock)
                        this.simulation.Bodies.ApplyDescription(this.handle, this.description);
            }
        }

        public KinematicBodyPhysicsElement(BodyHandle handle, BodyDescription description, PhysicsWorld physicsWorld, Simulation simulation) :
            base(handle, description, physicsWorld, simulation)
        {
        }
    }
}
