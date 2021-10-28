using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using RenderWareIo;
using SlipeServer.Physics.Assets;
using SlipeServer.Physics.Callbacks;
using SlipeServer.Physics.Entities;
using System;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Physics.Worlds
{
    public class PhysicsWorld : IDisposable
    {
        private readonly BufferPool pool;
        private readonly Simulation simulation;
        private readonly AssetCollection assetCollection;

        public PhysicsWorld(AssetCollection? assetCollection = null)
        {
            this.pool = new BufferPool();
            this.simulation = Simulation.Create(this.pool, new NoCollisionCallbacks(), new DemoPoseIntegratorCallbacks(), new PositionFirstTimestepper());

            this.assetCollection = assetCollection ?? new();
        }

        public void Dispose()
        {
            this.simulation.Dispose();
            GC.SuppressFinalize(this);
        }

        public PhysicsElement<StaticDescription, StaticHandle> AddStatic(PhysicsMesh mesh, Vector3 position, Quaternion rotation)
        {
            var description = new StaticDescription(position, mesh.meshIndex, 0.1f);
            description.Pose.Orientation = rotation;
            var handle = this.simulation.Statics.Add(description);
            return new StaticPhysicsElement(handle, description, this.simulation);
        }

        public RayHit? RayCast(Vector3 from, Vector3 direction, float length)
        {
            HitHandler handler = new();
            this.simulation.RayCast(from, direction, length, ref handler);
            return handler.Hit;
        }

        public PhysicsImg LoadImg(string path)
        {
            return new PhysicsImg(path);
        }

        public void Destroy(PhysicsElement<StaticDescription, StaticHandle> element) => this.simulation.Statics.Remove(element.handle);

        public PhysicsMesh CreateCylinder(float radius, float length)
        {
            var cylinder = new Cylinder(radius, length);
            var shape = this.simulation.Shapes.Add(cylinder);
            return new PhysicsMesh(shape);
        }

        public PhysicsMesh CreateMesh(PhysicsImg imgFile, string dffName)
        {
            var img = imgFile.imgFile.Img;
            var dffFile = new DffFile(img.DataEntries[dffName.ToLower()].Data);
            var dff = dffFile.Dff;

            return CreateMesh(dff);
        }

        public PhysicsMesh CreateMesh(PhysicsImg imgFile, string colFileName, string colName)
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

        public PhysicsMesh CreateMesh(RenderWareIo.Structs.Col.ColCombo colCombo)
        {
            return GetPhysicsMesh(GetMeshFromCollider(colCombo));
        }

        public PhysicsMesh CreateMesh(RenderWareIo.Structs.Col.ColCombo colCombo, RenderWareIo.Structs.Col.FaceGroup group)
        {
            return GetPhysicsMesh(GetMeshFromCollider(colCombo, group));
        }

        private PhysicsMesh GetPhysicsMesh<T>(T mesh) where T: unmanaged, IShape
        {
            var shape = this.simulation.Shapes.Add(mesh);
            return new PhysicsMesh(shape);
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

        private Compound GetMeshFromCollider(RenderWareIo.Structs.Col.ColCombo colCombo)
        {
            unsafe
            {
                var colTriangles = colCombo.Body.Faces;
                var colVertices = colCombo.Body.Vertices;

                var shapeCount = colCombo.Body.Spheres.Count + colCombo.Body.Boxes.Count + 1;
                var builder = new CompoundBuilder(this.pool, this.simulation.Shapes, shapeCount);

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
                    var mesh = new Mesh(triangles, meshScale, this.pool);
                    var meshShape = this.simulation.Shapes.Add(mesh);

                    builder.Add(meshShape, new RigidPose(), new Symmetric3x3(), 0);
                }


                foreach (var sphere in colCombo.Body.Spheres)
                {
                    var shape = this.simulation.Shapes.Add(new Sphere(sphere.Radius));
                    builder.Add(shape, new RigidPose(sphere.Center), new Symmetric3x3(), 0);
                }

                foreach (var box in colCombo.Body.Boxes)
                {
                    var size = box.Max - box.Min;
                    var shape = this.simulation.Shapes.Add(new Box(size.X, size.Y, size.Z));
                    builder.Add(shape, new RigidPose(box.Min + size * 0.5f), new Symmetric3x3(), 0);
                }

                builder.BuildKinematicCompound(out var children);

                var compound = new Compound(children);

                return compound;
            }
        }

        private Mesh GetMeshFromCollider(RenderWareIo.Structs.Col.ColCombo colCombo, RenderWareIo.Structs.Col.FaceGroup group)
        {
            unsafe
            {
                var colTriangles = colCombo.Body.Faces.Skip(group.StartFace).Take(group.EndFace - group.StartFace);
                var colVertices = colCombo.Body.Vertices;

                this.pool.Take(colTriangles.Count() * sizeof(Triangle), out var buffer);
                var triangles = new Buffer<Triangle>(buffer.Memory, colTriangles.Count());
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
                var mesh = new Mesh(triangles, meshScale, this.pool);

                return mesh;
            }
        }
    }
}
