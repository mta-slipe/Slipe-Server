using BepuPhysics.Collidables;

namespace SlipeServer.Physics.Entities
{
    public struct ConvexPhysicsMesh: IPhysicsMesh
    {
        public IShape Shape { get; }
        public TypedIndex MeshIndex { get; }

        internal ConvexPhysicsMesh(IConvexShape shape, TypedIndex meshIndex)
        {
            this.Shape = shape;
            this.MeshIndex = meshIndex;
        }
    }
}
