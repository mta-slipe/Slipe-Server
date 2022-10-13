using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlipeServer.Physics;

internal struct MultiHitHandler : IRayHitHandler
{
    private readonly List<RayHit> hits;
    public IEnumerable<RayHit> Hits => this.hits.AsReadOnly();

    public MultiHitHandler()
    {
        this.hits = new();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowTest(CollidableReference collidable) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowTest(CollidableReference collidable, int childIndex) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnRayHit(in RayData ray, ref float maximumT, float distance, in Vector3 normal, CollidableReference collidable, int childIndex)
    {
        this.hits.Add(new RayHit()
        {
            ray = ray,
            Normal = normal,
            distance = distance,
            Collidable = collidable,
        });        
    }
}
