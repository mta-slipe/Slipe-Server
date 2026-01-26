using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class ExplosionScriptDefinitions(IExplosionService explosionService)
{
    [ScriptFunctionDefinition("createExplosion")]
    public bool CreateExplosion(Vector3 position, int type, Player? creator = null)
    {
        if (Enum.IsDefined(typeof(ExplosionType), type))
        {
            explosionService.CreateExplosion(position, (ExplosionType)type, creator);
            return true;
        }
        return false;
    }
}
