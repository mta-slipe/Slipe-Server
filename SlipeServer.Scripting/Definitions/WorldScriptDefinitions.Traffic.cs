using SlipeServer.Server.Enums;
using System;

namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions
{
    private static readonly (string ns, string ew, TrafficLightState state)[] trafficLightColorMap =
    [
        ("green",  "red",    TrafficLightState.GreenRed),
        ("yellow", "red",    TrafficLightState.YellowRed),
        ("red",    "red",    TrafficLightState.AllRed),
        ("red",    "green",  TrafficLightState.RedGreen),
        ("red",    "yellow", TrafficLightState.RedYellow),
        ("green",  "green",  TrafficLightState.AllGreen),
        ("yellow", "yellow", TrafficLightState.AllYellow),
        ("yellow", "green",  TrafficLightState.YellowGreen),
        ("green",  "yellow", TrafficLightState.GreenYellow),
    ];

    private static TrafficLightState GetTrafficLightStateFromColors(string nsColor, string ewColor)
    {
        foreach (var (ns, ew, state) in trafficLightColorMap)
        {
            if (ns.Equals(nsColor, StringComparison.OrdinalIgnoreCase) &&
                ew.Equals(ewColor, StringComparison.OrdinalIgnoreCase))
                return state;
        }
        return TrafficLightState.GreenRed;
    }

    [ScriptFunctionDefinition("areTrafficLightsLocked")]
    public bool AreTrafficLightsLocked()
        => gameWorld.AreTrafficLightsForced;

    [ScriptFunctionDefinition("getTrafficLightState")]
    public int GetTrafficLightState()
        => (int)gameWorld.TrafficLightState;

    [ScriptFunctionDefinition("setTrafficLightState")]
    public bool SetTrafficLightState(object state, string? colorEW = null)
    {
        if (state is double number)
        {
            gameWorld.SetTrafficLightState((TrafficLightState)(int)number, gameWorld.AreTrafficLightsForced);
        }
        else if (state is string colorNS)
        {
            if (colorEW != null)
            {
                gameWorld.SetTrafficLightState(GetTrafficLightStateFromColors(colorNS, colorEW), true);
            }
            else if (colorNS.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                gameWorld.SetTrafficLightState(TrafficLightState.GreenRed, false);
            }
            else
            {
                gameWorld.SetTrafficLightState(TrafficLightState.AllOff, true);
            }
        }
        return true;
    }

    [ScriptFunctionDefinition("setTrafficLightsLocked")]
    public bool SetTrafficLightsLocked(bool locked)
    {
        gameWorld.SetTrafficLightState(gameWorld.TrafficLightState, locked);
        return true;
    }
}
