using SlipeServer.Packets.Definitions.Lua;

namespace SlipeServer.Scripting.Definitions;

public class SettingsRegistryScriptDefinitions(ISettingsRegistry registry)
{
    [ScriptFunctionDefinition("get")]
    public LuaValue? Get(string settingName)
    {
        var splits = settingName.Split('.', System.StringSplitOptions.RemoveEmptyEntries);
        var providedResourceName = splits.Length > 1 ? splits[0] : null;
        var resourceName = ScriptExecutionContext.Current?.Owner?.Name;
        if (providedResourceName == null && resourceName != null)
        {
            var scoped = registry.Get($"{resourceName}.{settingName}");
            if (scoped is not null && !scoped.IsNil)
                return scoped ?? false;
        }
        return registry.Get(settingName) ?? false;
    }

    [ScriptFunctionDefinition("set")]
    public bool Set(string settingName, LuaValue value)
    {
        registry.Set(settingName, value);
        return true;
    }
}
