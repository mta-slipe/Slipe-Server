using BepuPhysics;
using SlipeServer.Physics.Worlds;
using SlipeServer.Server.Extensions;

namespace SlipeServer.Physics.Entities;

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
            this.CoupledElement.Rotation = pose.Orientation.ToEulerFromBeppu();
        }
    }
}
