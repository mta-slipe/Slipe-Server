using SlipeServer.Packets.Definitions.Lua;

namespace SlipeServer.Scripting.Definitions;

public class SettingsRegistryScriptDefinitions(ISettingsRegistry registry)
{
    [ScriptFunctionDefinition("get")]
    public LuaValue? Get(string settingName) => registry.Get(settingName);

    [ScriptFunctionDefinition("set")]
    public bool Set(string settingName, LuaValue value)
    {
        registry.Set(settingName, value);
        return true;
    }
}
