using BepuPhysics;
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

        public PhysicsElement(THandle handle, TDescription description, Simulation simulation)
        {
            this.handle = handle;
            this.description = description;
            this.simulation = simulation;
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
