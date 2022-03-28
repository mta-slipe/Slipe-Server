using BepuPhysics;
using BepuPhysics.Collidables;

namespace SlipeServer.Physics.Entities;

public struct CompoundPhysicsMesh : IPhysicsMesh
{
    public IShape Shape => this.CompoundShape;
    public ICompoundShape CompoundShape { get; }
    public TypedIndex MeshIndex { get; }
    public BodyInertia Inertia { get; }

    internal CompoundPhysicsMesh(ICompoundShape shape, TypedIndex meshIndex, BodyInertia inertia)
    {
        this.CompoundShape = shape;
        this.MeshIndex = meshIndex;
        this.Inertia = inertia;
    }
}
