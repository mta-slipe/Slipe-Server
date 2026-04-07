using SlipeServer.Server.Enums;

namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions
{
    [ScriptFunctionDefinition("isGarageOpen")]
    public bool IsGarageOpen(int garageId)
        => gameWorld.IsGarageOpen((GarageLocation)garageId);

    [ScriptFunctionDefinition("setGarageOpen")]
    public bool SetGarageOpen(int garageId, bool open)
    {
        gameWorld.SetGarageOpen((GarageLocation)garageId, open);
        return true;
    }
}
