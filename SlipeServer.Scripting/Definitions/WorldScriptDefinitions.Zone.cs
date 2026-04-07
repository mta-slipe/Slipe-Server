using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions
{
    [ScriptFunctionDefinition("getZoneName")]
    public string GetZoneName(Vector3 position, bool citiesOnly = false)
        => zoneService.GetZoneName(position, citiesOnly);
}
