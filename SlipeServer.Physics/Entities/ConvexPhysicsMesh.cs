using BepuPhysics.Collidables;

namespace SlipeServer.Physics.Entities;

public struct ConvexPhysicsMesh : IPhysicsMesh
{
    public IShape Shape => this.ConvexShape;
    public IConvexShape ConvexShape { get; }
    public TypedIndex MeshIndex { get; }

    internal ConvexPhysicsMesh(IConvexShape shape, TypedIndex meshIndex)
    {
        this.ConvexShape = shape;
        this.MeshIndex = meshIndex;
    }
}
