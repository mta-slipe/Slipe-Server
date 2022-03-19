using BepuPhysics.Collidables;

namespace SlipeServer.Physics.Entities
{
    public struct PhysicsMesh : IPhysicsMesh
    {
        public IShape Shape { get;  }
        public TypedIndex MeshIndex { get; }

        internal PhysicsMesh(IShape shape, TypedIndex meshIndex)
        {
            this.Shape = shape;
            this.MeshIndex = meshIndex;
        }
    }
}
