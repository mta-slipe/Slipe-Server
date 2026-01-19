using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using Microsoft.Extensions.Logging;
using RenderWareIo;
using RenderWareIo.Structs.Dff;
using SlipeServer.Physics.Assets;
using SlipeServer.Physics.Callbacks;
using SlipeServer.Physics.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Sphere = BepuPhysics.Collidables.Sphere;
using Triangle = BepuPhysics.Collidables.Triangle;

namespace SlipeServer.Physics.Worlds;

public class PhysicsWorld : IDisposable
{
    private readonly BufferPool pool;
    private readonly Simulation simulation;
    private readonly ILogger logger;
    private readonly AssetCollection assetCollection;

    public readonly ReaderWriterLockSlim physicsLock = new();

    private bool running;
    private int sleepTime;

    public PhysicsWorld(ILogger logger, Vector3 gravity, AssetCollection? assetCollection = null)
    {
        this.logger = logger;
        this.pool = new BufferPool();
        this.simulation = Simulation.Create(this.pool, new NarrowPhaseCallbacks(), new SimplePoseIntegratorCallbacks(gravity), new PositionFirstTimestepper());

        this.assetCollection = assetCollection ?? new();
    }

    public void Dispose()
    {
        this.simulation.Dispose();
        GC.SuppressFinalize(this);
    }

    public PhysicsElement<StaticDescription, StaticHandle> AddStatic(IPhysicsMesh mesh, Vector3 position, Quaternion rotation)
    {
        var description = new StaticDescription(position, mesh.MeshIndex, 0.1f);
        description.Pose.Orientation = rotation;
        this.physicsLock.EnterWriteLock();
        try
        {
            var handle = this.simulation.Statics.Add(description);
            return new StaticPhysicsElement(handle, description, this, this.simulation);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    public DynamicBodyPhysicsElement AddDynamicBody(ConvexPhysicsMesh mesh, Vector3 position, Quaternion rotation, float mass, float friction = 0.1f)
    {
        mesh.ConvexShape.ComputeInertia(mass, out var inertia);
        var collidable = new CollidableDescription(mesh.MeshIndex, friction);
        return AddDynamicBody(collidable, inertia, position, rotation);
    }

    public DynamicBodyPhysicsElement AddDynamicBody(CompoundPhysicsMesh mesh, Vector3 position, Quaternion rotation, float mass, float friction = 0.1f)
    {
        var collidable = new CollidableDescription(mesh.MeshIndex, 0.1f);
        return AddDynamicBody(collidable, mesh.Inertia, position, rotation);
    }

    private DynamicBodyPhysicsElement AddDynamicBody(CollidableDescription collidable, BodyInertia inertia, Vector3 position, Quaternion rotation)
    {
        var pose = new RigidPose(position, rotation);
        var activityDescription = new BodyActivityDescription(0.1f);

        var description = BodyDescription.CreateDynamic(pose, inertia, collidable, activityDescription);
        description.Pose.Orientation = rotation;

        this.physicsLock.EnterWriteLock();
        try
        {
            var handle = this.simulation.Bodies.Add(description);
            return new DynamicBodyPhysicsElement(handle, description, this, this.simulation, activityDescription);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    public PhysicsElement<BodyDescription, BodyHandle> AddKinematicBody(ConvexPhysicsMesh mesh, Vector3 position, Quaternion rotation)
    {
        var collidable = new CollidableDescription(mesh.MeshIndex, 0.1f);
        return AddKinematicBody(collidable, position, rotation);
    }

    public PhysicsElement<BodyDescription, BodyHandle> AddKinematicBody(CompoundPhysicsMesh mesh, Vector3 position, Quaternion rotation)
    {
        var collidable = new CollidableDescription(mesh.MeshIndex, 0.1f);
        return AddKinematicBody(collidable, position, rotation);
    }

    private PhysicsElement<BodyDescription, BodyHandle> AddKinematicBody(CollidableDescription collidable, Vector3 position, Quaternion rotation)
    {
        var pose = new RigidPose(position, rotation);
        var velocity = new BodyVelocity();
        var activityDescription = new BodyActivityDescription(0.1f);

        var description = BodyDescription.CreateKinematic(pose, velocity, collidable, activityDescription);
        description.Pose.Orientation = rotation;

        this.physicsLock.EnterWriteLock();
        try
        {
            var handle = this.simulation.Bodies.Add(description);
            return new KinematicBodyPhysicsElement(handle, description, this, this.simulation);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    public RayHit? RayCast(Vector3 from, Vector3 direction, float length)
    {
        this.physicsLock.EnterReadLock();
        try
        {
            HitHandler handler = new();
            this.simulation.RayCast(from, direction, length, ref handler);
            return handler.Hit;
        } finally
        {
            this.physicsLock.ExitReadLock();
        }
    }

    public IEnumerable<RayHit> MultiRayCast(Vector3 from, Vector3 direction, float length)
    {
        this.physicsLock.EnterReadLock();
        try
        {
            MultiHitHandler handler = new();
            this.simulation.RayCast(from, direction, length, ref handler);
            return handler.Hits;
        } finally
        {
            this.physicsLock.ExitReadLock();
        }
    }

    public PhysicsImg LoadImg(string path)
    {
        return new PhysicsImg(path);
    }

    public void Destroy(PhysicsElement<StaticDescription, StaticHandle> element)
    {
        this.physicsLock.EnterWriteLock();
        try
        {
            this.simulation.Statics.Remove(element.handle);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    public void Destroy(PhysicsElement<BodyDescription, BodyHandle> element)
    {
        this.physicsLock.EnterWriteLock();
        try
        {
            this.simulation.Bodies.Remove(element.handle);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    public ConvexPhysicsMesh CreateSphere(float radius)
    {
        var sphere = new Sphere(radius);
        this.physicsLock.EnterWriteLock();
        try
        {
            var shape = this.simulation.Shapes.Add(sphere);
            return new ConvexPhysicsMesh(sphere, shape);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    public CompoundPhysicsMesh CreateCylinder(float radius, float length)
    {
        var cylinder = new Cylinder(radius, length);
        this.physicsLock.EnterWriteLock();
        try
        {
            unsafe
            {
                var shape = this.simulation.Shapes.Add(cylinder);
                var rotation = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), MathF.PI / 2);

                this.pool.Take<CompoundChild>(1, out var buffer);
                buffer[0] = new CompoundChild()
                {
                    ShapeIndex = shape,
                    LocalPose = new RigidPose(Vector3.Zero, rotation)
                };
                var compound = new Compound(buffer);

                var compoundShape = this.simulation.Shapes.Add(compound);

                cylinder.ComputeInertia(30, out var inertia);
                return new CompoundPhysicsMesh(compound, compoundShape, inertia);
            }
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    public PhysicsMesh CreateMesh(PhysicsImg imgFile, string dffName)
    {
        var img = imgFile.imgFile.Img;
        var dffFile = new DffFile(img.DataEntries[dffName.ToLower()].Data);
        var dff = dffFile.Dff;

        return CreateMesh(dff);
    }

    public (CompoundPhysicsMesh?, PhysicsMesh?) CreateMesh(PhysicsImg imgFile, string colFileName, string colName)
    {
        var img = imgFile.imgFile.Img;
        var colFile = new ColFile(img.DataEntries[colFileName.ToLower()].Data);
        var col = colFile.Col;
        var combo = col.ColCombos.First(x =>
        {
            var fullString = string.Join("", x.Header.Name);
            var name = fullString.Substring(0, fullString.IndexOf('\0'));
            return name == colName;
        });

        return CreateMesh(combo);
    }

    public PhysicsMesh CreateMesh(RenderWareIo.Structs.Dff.Dff dff)
    {
        return GetPhysicsMesh(GetMeshFromModel(dff));
    }

    public (CompoundPhysicsMesh?, PhysicsMesh?) CreateMesh(RenderWareIo.Structs.Col.ColCombo colCombo)
    {
        var (compound, mesh, inertia) = GetMeshFromCollider(colCombo);
        return (
            compound != null && inertia != null ? GetCompoundPhysicsMesh(compound.Value, inertia.Value) : (CompoundPhysicsMesh?)null,
            mesh != null ? GetPhysicsMesh(mesh.Value) : (PhysicsMesh?)null);
    }

    public ConvexPhysicsMesh CreateConvexMesh(PhysicsImg imgFile, string colFileName, string colName)
    {
        var img = imgFile.imgFile.Img;
        var colFile = new ColFile(img.DataEntries[colFileName.ToLower()].Data);
        var col = colFile.Col;
        var combo = col.ColCombos.First(x =>
        {
            var fullString = string.Join("", x.Header.Name);
            var name = fullString.Substring(0, fullString.IndexOf('\0'));
            return name == colName;
        });

        return CreateConvexMesh(combo);
    }

    public ConvexPhysicsMesh CreateConvexMesh(PhysicsImg imgFile, string dffName)
    {
        var img = imgFile.imgFile.Img;
        var dffFile = new DffFile(img.DataEntries[dffName.ToLower()].Data);

        return CreateConvexMesh(dffFile.Dff);
    }

    public ConvexPhysicsMesh CreateConvexMesh(Dff dff)
    {
        this.physicsLock.EnterWriteLock();
        try
        {
            var vertices = dff.Clump.GeometryList.Geometries.SelectMany(x => x.MorphTargets.SelectMany(y => y.Vertices));
            var hull = new ConvexHull(vertices.ToArray(), this.pool, out var center);

            var shape = this.simulation.Shapes.Add(hull);

            return new ConvexPhysicsMesh(hull, shape);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    public ConvexPhysicsMesh CreateConvexMesh(RenderWareIo.Structs.Col.ColCombo combo)
    {
        this.physicsLock.EnterWriteLock();
        try
        {
            var meshVertices = combo.Body.Vertices
            .Select(x => new Vector3(x.FirstFloat, x.SecondFloat, x.ThirdFloat));
            var boxVertices = combo.Body.Boxes
                .SelectMany(box =>
                {
                    var min = box.Min;
                    var max = box.Max;
                    return new List<Vector3>
                    {
                    new(min.X, min.Y, min.Z),
                    new(max.X, min.Y, min.Z),
                    new(min.X, max.Y, min.Z),
                    new(max.X, max.Y, min.Z),
                    new(min.X, min.Y, max.Z),
                    new(max.X, min.Y, max.Z),
                    new(min.X, max.Y, max.Z),
                    new(max.X, max.Y, max.Z),
                    };
                });

            var vertices = meshVertices.Concat(boxVertices);
            var hull = new ConvexHull(vertices.ToArray(), this.pool, out var center);

            var shape = this.simulation.Shapes.Add(hull);

            return new ConvexPhysicsMesh(hull, shape);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    public CompoundPhysicsMesh CreateConvexCompoundMesh(PhysicsImg imgFile, string colFileName, string colName)
    {
        var img = imgFile.imgFile.Img;
        var colFile = new ColFile(img.DataEntries[colFileName.ToLower()].Data);
        var col = colFile.Col;
        var combo = col.ColCombos.First(x =>
        {
            var fullString = string.Join("", x.Header.Name);
            var name = fullString.Substring(0, fullString.IndexOf('\0'));
            return name == colName;
        });

        return CreateConvexCompoundMesh(combo);
    }

    public CompoundPhysicsMesh CreateConvexCompoundMesh(RenderWareIo.Structs.Col.ColCombo combo)
    {
        this.physicsLock.EnterWriteLock();
        try
        {
            var meshVertices = combo.Body.Vertices
             .Select(x => new Vector3(x.FirstFloat, x.SecondFloat, x.ThirdFloat));

            var hull = new ConvexHull(meshVertices.ToArray(), this.pool, out var center);
            var hullShape = this.simulation.Shapes.Add(hull);

            var inverseInertia = new Symmetric3x3() { XX = 1, YY = 1, ZZ = 1 };

            var compoundBuilder = new CompoundBuilder(this.pool, this.simulation.Shapes, 1 + combo.Body.Boxes.Count + combo.Body.Spheres.Count);
            hull.ComputeInertia(10, out var hullInertia);
            Symmetric3x3.Invert(hullInertia.InverseInertiaTensor, out var hullLocalInertia);
            compoundBuilder.Add(hullShape, new RigidPose(center), hullLocalInertia, 100);

            foreach (var box in combo.Body.Boxes)
            {
                var size = box.Max - box.Min;
                var boxShape = new Box(size.X, size.Y, size.Z);

                var position = box.Min + size * 0.5f;

                var shape = this.simulation.Shapes.Add(boxShape);
                boxShape.ComputeInertia(10, out var boxInertia); 
                Symmetric3x3.Invert(boxInertia.InverseInertiaTensor, out var boxLocalInertia);
                compoundBuilder.Add(shape, new RigidPose(position), boxLocalInertia, 10);
            }

            foreach (var sphere in combo.Body.Spheres)
            {
                var sphereShape = new Sphere(sphere.Radius);
                var shape = this.simulation.Shapes.Add(sphereShape); 
                sphereShape.ComputeInertia(10, out var sphereInertia); 
                Symmetric3x3.Invert(sphereInertia.InverseInertiaTensor, out var sphereLocalInertia);
                compoundBuilder.Add(shape, new RigidPose(sphere.Center), sphereLocalInertia, 10);
            }

            compoundBuilder.BuildDynamicCompound(out var children, out var inertia);
            var compound = new Compound(children);
            var compoundShape = this.simulation.Shapes.Add(compound);

            return new CompoundPhysicsMesh(compound, compoundShape, inertia);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    public void Start(int sleepTime)
    {
        if (this.running)
            return;

        this.running = true;
        this.sleepTime = sleepTime;
        Task.Run(StepLoop);
    }

    public void Stop()
    {
        this.running = false;
    }

    public async Task StepLoop()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        while (this.running)
        {
            try
            {
                var deltaTime = stopwatch.ElapsedTicks * 0.00000025f;
                if (deltaTime > 0)
                {
                    stopwatch.Reset();
                    stopwatch.Start();
                    this.physicsLock.EnterWriteLock();
                    try
                    {
                        this.simulation.Timestep(deltaTime, null);
                    }
                    finally
                    {
                        this.physicsLock.ExitWriteLock();
                    }
                    this.Stepped?.Invoke();
                }
                await Task.Delay(this.sleepTime);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Physics error: {message}", e.Message);
            }
        }
    }

    private CompoundPhysicsMesh GetCompoundPhysicsMesh<T>(T mesh, BodyInertia inertia) where T : unmanaged, ICompoundShape
    {
        this.physicsLock.EnterWriteLock();
        try
        {
            var shape = this.simulation.Shapes.Add(mesh);
            return new CompoundPhysicsMesh(mesh, shape, inertia);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    private ConvexPhysicsMesh GetConvexPhysicsMesh<T>(T mesh) where T : unmanaged, IConvexShape
    {
        this.physicsLock.EnterWriteLock();
        try
        {
            var shape = this.simulation.Shapes.Add(mesh);
            return new ConvexPhysicsMesh(mesh, shape);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    private PhysicsMesh GetPhysicsMesh<T>(T mesh) where T : unmanaged, IShape
    {
        this.physicsLock.EnterWriteLock();
        try
        {
            var shape = this.simulation.Shapes.Add(mesh);
            return new PhysicsMesh(mesh, shape);
        }
        finally
        {
            this.physicsLock.ExitWriteLock();
        }
    }

    private Mesh GetMeshFromModel(RenderWareIo.Structs.Dff.Dff dff)
    {
        unsafe
        {
            var dffTriangles = dff.Clump.GeometryList.Geometries.First().Triangles;
            var dffVertices = dff.Clump.GeometryList.Geometries.First().MorphTargets.SelectMany(x => x.Vertices).ToArray();

            this.pool.Take(dffTriangles.Count * sizeof(Triangle), out var buffer);
            var triangles = new Buffer<Triangle>(buffer.Memory, dffTriangles.Count);
            int vertexIndex = 0;
            foreach (var triangle in dffTriangles)
            {
                triangles[vertexIndex++] = new Triangle(
                    dffVertices[triangle.VertexIndexOne],
                    dffVertices[triangle.VertexIndexTwo],
                    dffVertices[triangle.VertexIndexThree]);
            }

            var meshScale = Vector3.One;
            var mesh = new Mesh(triangles, meshScale, this.pool);

            return mesh;
        }
    }

    private (Compound?, Mesh?, BodyInertia?) GetMeshFromCollider(RenderWareIo.Structs.Col.ColCombo colCombo)
    {
        var inverseInertia = new Symmetric3x3() { XX = 1, YY = 1, ZZ = 1 };

        unsafe
        {
            var colTriangles = colCombo.Body.Faces;
            var colVertices = colCombo.Body.Vertices;

            var shapeCount = colCombo.Body.Spheres.Count + colCombo.Body.Boxes.Count;
            var builder = new CompoundBuilder(this.pool, this.simulation.Shapes, shapeCount);

            Mesh? mesh = null;
            if (colTriangles.Any())
            {
                this.pool.Take(colTriangles.Count * sizeof(Triangle), out var buffer);
                var triangles = new Buffer<Triangle>(buffer.Memory, colTriangles.Count);
                int vertexIndex = 0;
                foreach (var triangle in colTriangles)
                {
                    triangles[vertexIndex++] = new Triangle(
                        new Vector3(colVertices[triangle.A].FirstFloat, colVertices[triangle.A].SecondFloat, colVertices[triangle.A].ThirdFloat),
                        new Vector3(colVertices[triangle.B].FirstFloat, colVertices[triangle.B].SecondFloat, colVertices[triangle.B].ThirdFloat),
                        new Vector3(colVertices[triangle.C].FirstFloat, colVertices[triangle.C].SecondFloat, colVertices[triangle.C].ThirdFloat)
                    );
                }

                var meshScale = Vector3.One;
                mesh = new Mesh(triangles, meshScale, this.pool);
            }

            Compound? compound = null;
            BodyInertia? inertia = null;

            if (shapeCount > 0)
            {
                foreach (var sphere in colCombo.Body.Spheres)
                {
                    this.physicsLock.EnterWriteLock();
                    try
                    {
                        var shape = this.simulation.Shapes.Add(new BepuPhysics.Collidables.Sphere(sphere.Radius));
                        builder.Add(shape, new RigidPose(sphere.Center), inverseInertia, 10);
                    }
                    finally
                    {
                        this.physicsLock.ExitWriteLock();
                    }
                }

                foreach (var box in colCombo.Body.Boxes)
                {
                    this.physicsLock.EnterWriteLock();
                    try
                    {
                        var size = box.Max - box.Min;
                        var shape = this.simulation.Shapes.Add(new Box(size.X, size.Y, size.Z));
                        builder.Add(shape, new RigidPose(box.Min + size * 0.5f), inverseInertia, 10);
                    }
                    finally
                    {
                        this.physicsLock.ExitWriteLock();
                    }
                }

                builder.BuildDynamicCompound(out var children, out var bodyInertia);
                inertia = bodyInertia;

                compound = new Compound(children);
            }

            return (compound, mesh, inertia);
        }
    }

    public event Action? Stepped;
}
