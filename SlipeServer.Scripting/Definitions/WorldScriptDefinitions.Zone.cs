namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions
{
    [ScriptFunctionDefinition("getZoneName")]
    public string GetZoneName(float x, float y, float z, bool citiesOnly = false)
        => zoneService.GetZoneName(x, y, z, citiesOnly);
}
