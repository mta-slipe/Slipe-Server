﻿using Microsoft.Extensions.Logging;
using RenderWareIo;
using RenderWareIo.Structs.BinaryIpl;
using RenderWareIo.Structs.Col;
using RenderWareIo.Structs.Dff;
using RenderWareIo.Structs.Ide;
using RenderWareIo.Structs.Img;
using RenderWareIo.Structs.Ipl;
using SlipeServer.Physics.Assets;
using SlipeServer.Physics.Entities;
using SlipeServer.Physics.Enum;
using SlipeServer.Physics.Worlds;
using SlipeServer.Server.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SlipeServer.Physics.Builders;

public class PhysicsWorldBuilder
{
    private readonly ILogger logger;

    private readonly AssetCollection assetCollection;
    private readonly List<Action<PhysicsWorld>> actions;
    private readonly List<Img> imgs;
    private readonly Dictionary<Dff, IPhysicsMesh> dffMeshes;
    private readonly Dictionary<ColCombo, IPhysicsMesh[]> colMeshes;
    private readonly Dictionary<string, ColCombo> namedColCombos;
    private PhysicsModelLoadMode loadMode;
    private Vector3 gravity;

    public PhysicsWorldBuilder(ILogger logger)
    {
        this.logger = logger;

        this.assetCollection = new();
        this.actions = [];
        this.imgs = [];
        this.dffMeshes = [];
        this.colMeshes = [];
        this.namedColCombos = [];

        this.loadMode = PhysicsModelLoadMode.Col;
    }

    public void SetMode(PhysicsModelLoadMode loadMode)
    {
        this.loadMode = loadMode;
    }

    public void SetGravity(Vector3 gravity)
    {
        this.gravity = gravity;
    }

    public void AddImg(Img img)
    {
        this.imgs.Add(img);

        if (this.loadMode == PhysicsModelLoadMode.Col)
        {
            foreach (var entry in img.DataEntries)
            {
                if (entry.Key.EndsWith(".col"))
                {
                    try
                    {
                        var col = new ColFile(entry.Value.Data).Col;

                        foreach (var combo in col.ColCombos)
                        {
                            var fullString = string.Join("", combo.Header.Name);
                            var name = fullString.Substring(0, fullString.IndexOf('\0')).ToLower();
                            this.namedColCombos[name] = combo;
                        }
                    }
                    catch (Exception)
                    {
                        this.logger.LogTrace($"Unable to locate col {entry.Value.Data}");
                    }
                }
            }
        }
    }

    public void AddIde(string ideName, Ide ide, PhysicsModelLoadMode loadMode)
    {
        if (loadMode == PhysicsModelLoadMode.Dff)
        {
            foreach (var obj in ide.Objs)
            {
                var dff = GetDff(obj.ModelName + ".dff");
                if (dff != null)
                {
                    this.assetCollection.RegisterDff(obj.Id, dff);
                    this.actions.Add(world =>
                    {
                        try
                        {
                            this.dffMeshes[dff] = world.CreateMesh(dff);
                        }
                        catch (Exception)
                        {
                            this.logger.LogTrace($"Unable to locate dff {obj.ModelName}");
                        }
                    });
                } else
                {
                    this.logger.LogTrace($"Unable to get dff {obj.ModelName} from img");
                }
            }
        } else
        {
            foreach (var obj in ide.Objs)
            {
                if (this.namedColCombos.ContainsKey(obj.ModelName.ToLower()))
                {
                    var colCombo = this.namedColCombos[obj.ModelName.ToLower()];
                    if (colCombo.Header.BoxCount + colCombo.Header.SphereCount + colCombo.Header.FaceCount > 0)
                    {
                        this.assetCollection.RegisterCol(obj.Id, colCombo);
                        this.actions.Add(world =>
                        {
                            try
                            {
                                var meshes = world.CreateMesh(colCombo);
                                var resultMeshes = new List<IPhysicsMesh>();
                                if (meshes.Item1 != null)
                                {
                                    resultMeshes.Add(meshes.Item1);
                                }
                                if (meshes.Item2 != null)
                                {
                                    resultMeshes.Add(meshes.Item2);
                                }
                                this.colMeshes[colCombo] = resultMeshes.ToArray();
                            }
                            catch (Exception)
                            {
                                this.logger.LogTrace($"Unable to locate col {obj.ModelName.ToLower()} in {ideName}");
                            }
                        });
                    }
                } else
                {
                    this.logger.LogTrace($"Unable to find col {obj.ModelName.ToLower()} in {ideName}");
                }
            }
        }
    }

    public void AddIpl(Ipl ipl, PhysicsModelLoadMode loadMode)
    {
        foreach (var inst in ipl.Insts)
        {
            if (loadMode == PhysicsModelLoadMode.Dff)
            {
                this.actions.Add(world =>
                {
                    var dff = this.assetCollection.GetDff(inst.Id);
                    if (dff != null && this.dffMeshes.TryGetValue(dff, out var mesh))
                    {
                        var euler = inst.Rotation.ToEuler();
                        euler = new Vector3(euler.X, euler.Y, -euler.Z);
                        world.AddStatic(mesh, inst.Position, euler.ToQuaternion());
                    }
                });
            } else
            {
                this.actions.Add(world =>
                {
                    var col = this.assetCollection.GetCol(inst.Id);
                    if (col != null && this.colMeshes.TryGetValue(col, out var meshes))
                    {
                        foreach (var mesh in meshes)
                        {
                            var euler = inst.Rotation.ToEuler();
                            euler = new Vector3(euler.X, euler.Y, -euler.Z);
                            world.AddStatic(mesh, inst.Position, euler.ToQuaternion());
                        }
                    }
                });
            }
        }
    }

    public void AddIpl(BinaryIpl ipl, PhysicsModelLoadMode loadMode)
    {
        foreach (var inst in ipl.Insts)
        {
            if (loadMode == PhysicsModelLoadMode.Dff)
            {
                this.actions.Add(world =>
                {
                    var dff = this.assetCollection.GetDff(inst.Id);
                    if (dff != null && this.dffMeshes.TryGetValue(dff, out var mesh))
                    {
                        var euler = inst.Rotation.ToEuler();
                        euler = new Vector3(euler.X, euler.Y, -euler.Z);
                        world.AddStatic(mesh, inst.Position, euler.ToQuaternion());
                    } else
                    {
                        this.logger.LogTrace("Dff not found for {dff}", inst.Id);
                    }
                });
            } else
            {
                this.actions.Add(world =>
                {
                    var col = this.assetCollection.GetCol(inst.Id);
                    if (col != null && this.colMeshes.TryGetValue(col, out var meshes))
                    {
                        foreach (var mesh in meshes)
                        {
                            var euler = inst.Rotation.ToEuler();
                            euler = new Vector3(euler.X, euler.Y, -euler.Z);
                            world.AddStatic(mesh, inst.Position, euler.ToQuaternion());
                        }
                    } else
                    {
                        this.logger.LogTrace("Col not found for {col}", inst.Id);
                    }
                });
            }
        }
    }

    public PhysicsWorld Build()
    {
        var world = new PhysicsWorld(this.logger, this.gravity, this.assetCollection);

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
        name = name.ToLower().Trim('\0');
        foreach (var img in this.imgs)
        {
            if (img.DataEntries.TryGetValue(name, out var value))
                return value.Data;
        }
        return null;
    }
}
