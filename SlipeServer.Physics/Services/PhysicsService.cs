using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using RenderWareIo;
using SlipeServer.Physics.Callbacks;
using SlipeServer.Physics.Entities;
using System.Linq;
using System.Numerics;

namespace SlipeServer.Physics.Services
{

    public partial class PhysicsService
    {
        private readonly BufferPool pool;
        private readonly Simulation simulation;


        public PhysicsService()
        {
            this.pool = new BufferPool();
            this.simulation = Simulation.Create(this.pool, new NoCollisionCallbacks(), new DemoPoseIntegratorCallbacks(), new PositionFirstTimestepper());
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

        public PhysicsMesh CreateMesh(PhysicsImg imgFile, string dffName)
        {
            var img = imgFile.imgFile.Img;
            var dffFile = new DffFile(img.DataEntries[dffName.ToLower()].Data);
            var dff = dffFile.Dff;

            var mesh = GetMeshFromModel(dff);
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
    }
}
