using BepuPhysics;
using SlipeServer.Physics.Worlds;
using System.Numerics;

namespace SlipeServer.Physics.Entities
{
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
