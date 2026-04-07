using SlipeServer.Server.Enums;

namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions
{
    [ScriptFunctionDefinition("areTrafficLightsLocked")]
    public bool AreTrafficLightsLocked()
        => gameWorld.AreTrafficLightsForced;

    [ScriptFunctionDefinition("getTrafficLightState")]
    public int GetTrafficLightState()
        => (int)gameWorld.TrafficLightState;

    [ScriptFunctionDefinition("setTrafficLightState")]
    public bool SetTrafficLightState(int state)
    {
        gameWorld.SetTrafficLightState((TrafficLightState)state, gameWorld.AreTrafficLightsForced);
        return true;
    }

    [ScriptFunctionDefinition("setTrafficLightsLocked")]
    public bool SetTrafficLightsLocked(bool locked)
    {
        gameWorld.SetTrafficLightState(gameWorld.TrafficLightState, locked);
        return true;
    }
}
