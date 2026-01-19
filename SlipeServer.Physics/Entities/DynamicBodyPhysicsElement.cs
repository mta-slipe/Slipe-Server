using BepuPhysics;
using SlipeServer.Physics.Worlds;
using SlipeServer.Server.Extensions;
using System;

namespace SlipeServer.Physics.Entities;

public class DynamicBodyPhysicsElement : PhysicsElement<BodyDescription, BodyHandle>
{
    private readonly BodyActivityDescription bodyActivity;

    public DynamicBodyPhysicsElement(BodyHandle handle, BodyDescription description, PhysicsWorld physicsWorld, Simulation simulation, BodyActivityDescription bodyActivity) :
        base(handle, description, physicsWorld, simulation)
    {
        physicsWorld.Stepped += HandlePhysicsWorldStep;
        this.bodyActivity = bodyActivity;
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

    public void Apply(Action<BodyReference> action)
    {
        var body = this.simulation.Bodies.GetBodyReference(this.handle);
        action(body);
        body.Awake = true;
    }
}
