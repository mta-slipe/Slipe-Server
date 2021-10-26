using BepuPhysics;
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

        protected readonly Simulation simulation;

        protected virtual Vector3 Position { get; set; }
        protected virtual Quaternion Rotation { get; set; }

        public Element? CoupledElement { get; private set; }

        public PhysicsElement(THandle handle, TDescription description, Simulation simulation)
        {
            this.handle = handle;
            this.description = description;
            this.simulation = simulation;
        }

        public void CoupleWith(Element element)
        {
            if (this.CoupledElement != null)
            {
                Decouple();
            }
            this.CoupledElement = element;

            element.PositionChanged += HandleCoupledElementPositionUpdate;
            element.RotationChanged += HandleCoupledElementRotationUpdate;
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
            this.Position = args.NewValue;
        }

        protected virtual void HandleCoupledElementRotationUpdate(Element sender, ElementChangedEventArgs<Vector3> args)
        {
            this.Rotation = args.NewValue.ToQuaternion();
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
                this.simulation.Statics.ApplyDescription(this.handle, this.description);
            }
        }

        protected override Quaternion Rotation
        {
            get => this.description.Pose.Orientation;
            set
            {
                this.description.Pose.Orientation = value;
                this.simulation.Statics.ApplyDescription(this.handle, this.description);
            }
        }

        public StaticPhysicsElement(StaticHandle handle, StaticDescription description, Simulation simulation) : base(handle, description, simulation)
        {
        }
    }
}
