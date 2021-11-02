using BepuPhysics.Collidables;

namespace SlipeServer.Physics.Entities
{
    public interface IPhysicsMesh
    {
        IShape Shape { get; }
        TypedIndex MeshIndex { get; }
    }
}
