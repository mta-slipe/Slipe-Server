using BepuPhysics;
using SlipeServer.Physics.Worlds;
using System.Numerics;

namespace SlipeServer.Physics.Entities;

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
