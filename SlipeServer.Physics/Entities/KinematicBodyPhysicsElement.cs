using BepuPhysics;
using SlipeServer.Physics.Worlds;
using System.Numerics;

namespace SlipeServer.Physics.Entities;

public class KinematicBodyPhysicsElement(BodyHandle handle, BodyDescription description, PhysicsWorld physicsWorld, Simulation simulation) : PhysicsElement<BodyDescription, BodyHandle>(handle, description, physicsWorld, simulation)
{
    protected override Vector3 Position
    {
        get => this.description.Pose.Position;
        set
        {
            this.description.Pose.Position = value;
            lock (this.positionUpdateLock)
            {
                this.physicsWorld.physicsLock.EnterWriteLock();
                try
                {
                    this.simulation.Bodies.ApplyDescription(this.handle, this.description);
                }
                finally
                {
                    this.physicsWorld.physicsLock.ExitWriteLock();
                }
            }
        }
    }

    protected override Quaternion Rotation
    {
        get => this.description.Pose.Orientation;
        set
        {
            this.description.Pose.Orientation = value;
            lock (this.positionUpdateLock)
            {
                this.physicsWorld.physicsLock.EnterWriteLock();
                try
                {
                    this.simulation.Bodies.ApplyDescription(this.handle, this.description);
                }
                finally
                {
                    this.physicsWorld.physicsLock.ExitWriteLock();
                }
            }
        }
    }
}
