using SlipeServer.Server;
using SlipeServer.Server.Elements;
using System;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class ObjectScriptDefinitions(IMtaServer server)
{
    [ScriptFunctionDefinition("createObject")]
    public WorldObject CreateObject(ushort model, Vector3 position, Vector3? rotation = null, bool isLowLod = false)
    {
        var worldObject = new WorldObject(model, position)
        {
            Rotation = rotation ?? Vector3.Zero,
            IsLowLod = isLowLod
        }.AssociateWith(server);

        if (ScriptExecutionContext.Current?.Owner != null)
            worldObject.Parent = ScriptExecutionContext.Current.Owner?.DynamicRoot;

        return worldObject;
    }

    [ScriptFunctionDefinition("moveObject")]
    public bool MoveObject(WorldObject theObject, int time, Vector3 targetPosition, Vector3? deltaRotation = null, string? easingType = null, float? easingPeriod = null, float? easingAmplitude = null, float? easingOvershoot = null)
    {
        theObject.Move(targetPosition, deltaRotation ?? Vector3.Zero, TimeSpan.FromMilliseconds(time));
        return true;
    }

    [ScriptFunctionDefinition("isObjectMoving")]
    public bool IsObjectMoving(WorldObject theObject) => theObject.Movement != null;

    [ScriptFunctionDefinition("stopObject")]
    public bool StopObject(WorldObject theObject)
    {
        theObject.CancelMovement();
        return true;
    }

    [ScriptFunctionDefinition("getObjectScale")]
    public Vector3 GetObjectScale(WorldObject theObject) => theObject.Scale;

    [ScriptFunctionDefinition("setObjectScale")]
    public bool SetObjectScale(WorldObject theObject, float scale, float? scaleY = null, float? scaleZ = null)
    {
        theObject.Scale = new Vector3(scale, scaleY ?? scale, scaleZ ?? scale);
        return true;
    }

    [ScriptFunctionDefinition("isObjectBreakable")]
    public bool IsObjectBreakable(WorldObject theObject) => theObject.IsBreakable;

    [ScriptFunctionDefinition("setObjectBreakable")]
    public bool SetObjectBreakable(WorldObject theObject, bool breakable)
    {
        theObject.IsBreakable = breakable;
        return true;
    }

    [ScriptFunctionDefinition("breakObject")]
    public bool BreakObject(WorldObject theObject)
    {
        theObject.IsBroken = true;
        return true;
    }

    [ScriptFunctionDefinition("respawnObject")]
    public bool RespawnObject(WorldObject theObject)
    {
        theObject.IsBroken = false;
        return true;
    }

    [ScriptFunctionDefinition("toggleObjectRespawn")]
    public bool ToggleObjectRespawn(WorldObject theObject, bool respawn)
    {
        theObject.IsRespawnable = respawn;
        return true;
    }

    [ScriptFunctionDefinition("isObjectRespawnable")]
    public bool IsObjectRespawnable(WorldObject theObject) => theObject.IsRespawnable;
}
