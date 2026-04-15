using SlipeServer.Packets.Definitions.Lua;

namespace SlipeServer.Scripting.Definitions;

public class SettingsRegistryScriptDefinitions(ISettingsRegistry registry)
{
    [ScriptFunctionDefinition("get")]
    public LuaValue? Get(string settingName)
    {
        var resourceName = ScriptExecutionContext.Current?.Owner?.Name;
        if (resourceName != null)
        {
            var scoped = registry.Get($"{resourceName}.{settingName}");
            if (scoped != null && !scoped.IsNil)
                return scoped;
        }
        return registry.Get(settingName);
    }

    [ScriptFunctionDefinition("set")]
    public bool Set(string settingName, LuaValue value)
    {
        registry.Set(settingName, value);
        return true;
    }
}
