using BepuPhysics.Collidables;

namespace SlipeServer.Physics.Entities
{
    public struct PhysicsMesh
    {
        internal TypedIndex meshIndex;

        internal PhysicsMesh(TypedIndex meshIndex)
        {
            this.meshIndex = meshIndex;
        }
    }
}
