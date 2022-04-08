using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Physics;

internal struct HitHandler : IRayHitHandler
{
    public RayHit? Hit { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowTest(CollidableReference collidable) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowTest(CollidableReference collidable, int childIndex) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnRayHit(in RayData ray, ref float maximumT, float distance, in Vector3 normal, CollidableReference collidable, int childIndex)
    {
        if (!this.Hit.HasValue || this.Hit?.distance > distance)
        {
            this.Hit = new RayHit()
            {
                ray = ray,
                Normal = normal,
                distance = distance,
                Collidable = collidable
            };
        }
    }
}
