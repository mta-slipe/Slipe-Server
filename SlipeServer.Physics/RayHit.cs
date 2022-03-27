using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using System.Numerics;

namespace SlipeServer.Physics;

public struct RayHit
{
    public RayData ray;
    public Vector3 Normal;
    public float distance;
    public CollidableReference Collidable;
    public bool Hit;
}
