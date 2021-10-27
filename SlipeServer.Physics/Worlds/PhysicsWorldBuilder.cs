using RenderWareIo;
using RenderWareIo.Structs.BinaryIpl;
using RenderWareIo.Structs.Col;
using RenderWareIo.Structs.Dff;
using RenderWareIo.Structs.Ide;
using RenderWareIo.Structs.Img;
using RenderWareIo.Structs.Ipl;
using SlipeServer.Physics.Assets;
using SlipeServer.Physics.Entities;
using SlipeServer.Physics.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Physics.Builders
{
    public class PhysicsWorldBuilder
    {
        private readonly AssetCollection assetCollection;
        private readonly List<Action<PhysicsWorld>> actions;
        private readonly List<Img> imgs;
        private readonly Dictionary<Dff, PhysicsMesh> dffMeshes;
        private readonly Dictionary<Col, PhysicsMesh> colMeshes;

        public PhysicsWorldBuilder()
        {
            this.assetCollection = new();
            this.actions = new();
            this.imgs = new();
            this.dffMeshes = new();
            this.colMeshes = new();
        }

        public void AddImg(Img img)
        {
            this.imgs.Add(img);
        }

        public void AddIde(Ide ide)
        {
            foreach (var obj in ide.Objs)
            {
                var dff = GetDff(obj.ModelName + ".dff");
                if (dff != null)
                {
                    this.assetCollection.RegisterDff(obj.Id, dff);
                    this.actions.Add(world => {
                        try
                        {
                            this.dffMeshes[dff] = world.CreateMesh(dff);
                        } catch (Exception e)
                        {
                            System.Console.WriteLine(obj.ModelName);
                        }
                    });
                }
            }
        }

        public void AddIpl(Ipl ipl)
        {
            foreach (var inst in ipl.Insts)
            {
                this.actions.Add(world =>
                {
                    var dff = this.assetCollection.GetDff(inst.Id);
                    if (dff != null && this.dffMeshes.TryGetValue(dff, out var mesh))
                    {
                        world.AddStatic(mesh, inst.Position, inst.Rotation);
                    }
                });
            }
        }

        public void AddIpl(BinaryIpl ipl)
        {
            foreach (var inst in ipl.Insts)
            {
                this.actions.Add(world =>
                {
                    var dff = this.assetCollection.GetDff(inst.Id);
                    if (dff != null && this.dffMeshes.TryGetValue(dff, out var mesh))
                    {
                        world.AddStatic(mesh, inst.Position, inst.Rotation);
                    }
                });
            }
        }

        public PhysicsWorld Build()
        {
            var world = new PhysicsWorld(this.assetCollection);

            foreach (var action in this.actions)
                action(world);

            return world;
        }

        private Dff? GetDff(string name)
        {
            var asset = GetAssetFromImg(name);
            if (asset == null)
                return null;
            return new DffFile(asset).Dff;
        }

        private Col? GetCol(string name)
        {
            var asset = GetAssetFromImg(name);
            if (asset == null)
                return null;
            return new ColFile(asset).Col;
        }

        private byte[]? GetAssetFromImg(string name)
        {
            foreach (var img in this.imgs)
            {
                if (img.DataEntries.TryGetValue(name, out var value))
                    return value.Data;
            }
            return null;
        }
    }
}
