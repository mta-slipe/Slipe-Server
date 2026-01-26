using BepuPhysics;
using SlipeServer.Physics.Worlds;
using System.Numerics;

namespace SlipeServer.Physics.Entities;

public class StaticPhysicsElement(StaticHandle handle, StaticDescription description, PhysicsWorld physicsWorld, Simulation simulation) : PhysicsElement<StaticDescription, StaticHandle>(handle, description, physicsWorld, simulation)
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
                    this.simulation.Statics.ApplyDescription(this.handle, this.description);
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
                    this.simulation.Statics.ApplyDescription(this.handle, this.description);
                }
                finally
                {
                    this.physicsWorld.physicsLock.ExitWriteLock();
                }
            }
        }
    }
}
