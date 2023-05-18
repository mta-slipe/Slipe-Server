﻿using MoonSharp.Interpreter;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class ObjectScriptDefinitions
{
    private readonly MtaServer server;

    public ObjectScriptDefinitions(MtaServer server)
    {
        this.server = server;
    }

    [ScriptFunctionDefinition("createObject")]
    public WorldObject CreateObject(ushort model, Vector3 position, Vector3? rotation = null, bool isLowLod = false)
    {
        return new WorldObject(model, position)
        {
            Rotation = rotation ?? Vector3.Zero,
            IsLowLod = isLowLod
        }.AssociateWith(this.server);
    }


}
