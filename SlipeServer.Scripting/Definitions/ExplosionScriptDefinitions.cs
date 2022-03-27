using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public class ExplosionScriptDefinitions
{
    private readonly ExplosionService explosionService;

    public ExplosionScriptDefinitions(ExplosionService explosionService)
    {
        this.explosionService = explosionService;
    }

    [ScriptFunctionDefinition("createExplosion")]
    public bool CreateExplosion(Vector3 position, int type, Player? creator = null)
    {
        if (Enum.IsDefined(typeof(ExplosionType), type))
        {
            this.explosionService.CreateExplosion(position, (ExplosionType)type, creator);
            return true;
        }
        return false;
    }
}
