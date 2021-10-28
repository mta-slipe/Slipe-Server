using BepuPhysics;
using BepuUtilities;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Physics.Callbacks
{
    public struct SimplePoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        public Vector3 Gravity;
        public float LinearDamping;
        public float AngularDamping;

        private Vector3 gravityDt;
        private float linearDampingDt;
        private float angularDampingDt;

        public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

        public void Initialize(Simulation simulation)
        {
        }
            
        public SimplePoseIntegratorCallbacks(Vector3 gravity, float linearDamping = .03f, float angularDamping = .03f) : this()
        {
            this.Gravity = gravity;
            this.LinearDamping = linearDamping;
            this.AngularDamping = angularDamping;
        }

        public void PrepareForIntegration(float dt)
        {
            this.gravityDt = this.Gravity * dt;
            this.linearDampingDt = MathF.Pow(MathHelper.Clamp(1 - this.LinearDamping, 0, 1), dt);
            this.angularDampingDt = MathF.Pow(MathHelper.Clamp(1 - this.AngularDamping, 0, 1), dt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
        {
            if (localInertia.InverseMass > 0)
            {
                velocity.Linear = (velocity.Linear + this.gravityDt) * this.linearDampingDt;
                velocity.Angular *= this.angularDampingDt;
            }
        }

    }
}
