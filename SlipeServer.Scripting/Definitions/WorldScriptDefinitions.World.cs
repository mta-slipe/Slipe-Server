using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions
{
    [ScriptFunctionDefinition("getGravity")]
    public float GetGravity()
        => gameWorld.Gravity;

    [ScriptFunctionDefinition("setGravity")]
    public bool SetGravity(float gravity)
    {
        gameWorld.Gravity = gravity;
        return true;
    }

    [ScriptFunctionDefinition("getAircraftMaxHeight")]
    public float GetAircraftMaxHeight()
        => gameWorld.AircraftMaxHeight;

    [ScriptFunctionDefinition("setAircraftMaxHeight")]
    public bool SetAircraftMaxHeight(float height)
    {
        gameWorld.AircraftMaxHeight = height;
        return true;
    }

    [ScriptFunctionDefinition("getAircraftMaxVelocity")]
    public float GetAircraftMaxVelocity()
        => gameWorld.AircraftMaxVelocity;

    [ScriptFunctionDefinition("setAircraftMaxVelocity")]
    public bool SetAircraftMaxVelocity(float velocity)
    {
        gameWorld.AircraftMaxVelocity = velocity;
        return true;
    }

    [ScriptFunctionDefinition("getOcclusionsEnabled")]
    public bool GetOcclusionsEnabled()
        => gameWorld.OcclusionsEnabled;

    [ScriptFunctionDefinition("setOcclusionsEnabled")]
    public bool SetOcclusionsEnabled(bool enabled)
    {
        gameWorld.OcclusionsEnabled = enabled;
        return true;
    }

    [ScriptFunctionDefinition("getMoonSize")]
    public int GetMoonSize()
        => gameWorld.MoonSize;

    [ScriptFunctionDefinition("setMoonSize")]
    public bool SetMoonSize(int size)
    {
        gameWorld.MoonSize = size;
        return true;
    }

    [ScriptFunctionDefinition("resetMoonSize")]
    public bool ResetMoonSize()
    {
        gameWorld.MoonSize = 3;
        return true;
    }

    [ScriptFunctionDefinition("getJetpackMaxHeight")]
    public float GetJetpackMaxHeight()
        => gameWorld.MaxJetpackHeight;

    [ScriptFunctionDefinition("setJetpackMaxHeight")]
    public bool SetJetpackMaxHeight(float height)
    {
        gameWorld.MaxJetpackHeight = height;
        return true;
    }

    [ScriptFunctionDefinition("getInteriorSoundsEnabled")]
    public bool GetInteriorSoundsEnabled()
        => gameWorld.AreInteriorSoundsEnabled;

    [ScriptFunctionDefinition("setInteriorSoundsEnabled")]
    public bool SetInteriorSoundsEnabled(bool enabled)
    {
        gameWorld.AreInteriorSoundsEnabled = enabled;
        return true;
    }

    [ScriptFunctionDefinition("resetWorldProperties")]
    public void ResetWorldProperties(
        bool resetSpecialProperties = true,
        bool resetWorldProperties = true,
        bool resetWeatherProperties = true,
        bool resetLODs = true,
        bool resetSounds = true,
        bool resetGlitches = true,
        bool resetJetpackWeapons = true)
    {
        if (resetSpecialProperties)
            gameWorld.ResetSpecialProperties();

        if (resetGlitches)
            gameWorld.ResetGlitches();

        if (resetJetpackWeapons)
            gameWorld.ResetJetpackWeapons();

        if (resetWorldProperties)
        {
            gameWorld.GameSpeed = 1.0f;
            gameWorld.Gravity = 0.008f;
            gameWorld.OcclusionsEnabled = true;
            gameWorld.MoonSize = 3;
            gameWorld.RainLevel = 0;
            gameWorld.SunSize = 1;
            gameWorld.WindVelocity = Vector3.Zero;
            gameWorld.AircraftMaxHeight = 800.0f;
            gameWorld.AircraftMaxVelocity = 1.5f;
            gameWorld.MaxJetpackHeight = 100.0f;
            gameWorld.MinuteDuration = 1000;
        }

        if (resetWeatherProperties)
        {
            gameWorld.HeatHaze = null;
            gameWorld.CloudsEnabled = true;
            gameWorld.SetSkyGradient(Color.FromArgb(0, 0, 0), Color.FromArgb(0, 0, 255));
            gameWorld.SetSunColor(Color.FromArgb(255, 255, 255), Color.FromArgb(255, 200, 150));
            gameWorld.SetWeather((byte)0);
        }

        if (resetLODs)
        {
            gameWorld.FarClipDistance = null;
            gameWorld.FogDistance = null;
        }

        if (resetSounds)
        {
            gameWorld.AreInteriorSoundsEnabled = true;
        }
    }

    [ScriptFunctionDefinition("removeGameWorld")]
    public void RemoveGameWorld()
    {
        // Removes the game world on connected clients (network-level operation, stub)
    }

    [ScriptFunctionDefinition("restoreGameWorld")]
    public void RestoreGameWorld()
    {
        // Restores the game world on connected clients (network-level operation, stub)
    }
}
