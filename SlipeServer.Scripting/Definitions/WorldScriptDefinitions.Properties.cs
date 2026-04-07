using SlipeServer.Server.Enums;
using System;
using static SlipeServer.Server.Services.IGameWorld;

namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions
{
    [ScriptFunctionDefinition("isWorldSpecialPropertyEnabled")]
    public bool IsWorldSpecialPropertyEnabled(string propertyName)
    {
        if (!specialPropertyNames.TryGetValue(propertyName, out var property))
            throw new ArgumentException($"Unknown world special property: {propertyName}", nameof(propertyName));
        return gameWorld.SpecialPropertyStates.TryGetValue(property, out var enabled) && enabled;
    }

    [ScriptFunctionDefinition("setWorldSpecialPropertyEnabled")]
    public bool SetWorldSpecialPropertyEnabled(string propertyName, bool enabled)
    {
        if (!specialPropertyNames.TryGetValue(propertyName, out var property))
            throw new ArgumentException($"Unknown world special property: {propertyName}", nameof(propertyName));
        gameWorld.SetSpecialPropertyEnabled(property, enabled);
        return true;
    }

    [ScriptFunctionDefinition("getJetpackWeaponEnabled")]
    public bool GetJetpackWeaponEnabled(int weaponId)
        => gameWorld.IsJetpackWeaponEnabled((WeaponId)weaponId);

    [ScriptFunctionDefinition("setJetpackWeaponEnabled")]
    public bool SetJetpackWeaponEnabled(int weaponId, bool enabled)
    {
        gameWorld.SetJetpackWeaponEnabled((WeaponId)weaponId, enabled);
        return true;
    }
}
