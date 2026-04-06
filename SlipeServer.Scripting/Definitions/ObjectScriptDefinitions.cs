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
}
