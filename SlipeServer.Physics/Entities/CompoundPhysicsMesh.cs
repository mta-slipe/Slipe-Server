using BepuPhysics.Collidables;

namespace SlipeServer.Physics.Entities
{
    public struct CompoundPhysicsMesh : IPhysicsMesh
    {
        public IShape Shape { get;  }
        public TypedIndex MeshIndex { get; }

        internal CompoundPhysicsMesh(ICompoundShape shape, TypedIndex meshIndex)
        {
            this.Shape = shape;
            this.MeshIndex = meshIndex;
        }
    }
}
