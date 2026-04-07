using SlipeServer.Server.Structs;
using System.Drawing;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions
{
    [ScriptFunctionDefinition("getHeatHaze")]
    public (int, int, int, int, int, int, int, int, bool) GetHeatHaze()
    {
        var h = gameWorld.HeatHaze ?? new HeatHaze();
        return (
            h.Intensity,
            h.RandomShift,
            h.MinSpeed,
            h.MaxSpeed,
            (int)h.ScanSize.X,
            (int)h.ScanSize.Y,
            (int)h.RenderSize.X,
            (int)h.RenderSize.Y,
            h.IsEnabledInsideBuildings
        );
    }

    [ScriptFunctionDefinition("setHeatHaze")]
    public bool SetHeatHaze(
        int intensity,
        int randomShift = 0,
        int speedMin = 12,
        int speedMax = 18,
        int scanSizeX = 75,
        int scanSizeY = 80,
        int renderSizeX = 80,
        int renderSizeY = 85,
        bool showInside = false)
    {
        gameWorld.HeatHaze = new HeatHaze
        {
            Intensity = (byte)intensity,
            RandomShift = (byte)randomShift,
            MinSpeed = (ushort)speedMin,
            MaxSpeed = (ushort)speedMax,
            ScanSize = new Vector2(scanSizeX, scanSizeY),
            RenderSize = new Vector2(renderSizeX, renderSizeY),
            IsEnabledInsideBuildings = showInside,
        };
        return true;
    }

    [ScriptFunctionDefinition("resetHeatHaze")]
    public bool ResetHeatHaze()
    {
        gameWorld.HeatHaze = null;
        return true;
    }

    [ScriptFunctionDefinition("getCloudsEnabled")]
    public bool GetCloudsEnabled()
        => gameWorld.CloudsEnabled;

    [ScriptFunctionDefinition("setCloudsEnabled")]
    public bool SetCloudsEnabled(bool enabled)
    {
        gameWorld.CloudsEnabled = enabled;
        return true;
    }

    [ScriptFunctionDefinition("getSkyGradient")]
    public (int, int, int, int, int, int) GetSkyGradient()
    {
        var gradient = gameWorld.GetSkyGradient();
        if (gradient == null)
            return (0, 0, 0, 0, 0, 255);
        return (
            gradient.Value.Item1.R, gradient.Value.Item1.G, gradient.Value.Item1.B,
            gradient.Value.Item2.R, gradient.Value.Item2.G, gradient.Value.Item2.B
        );
    }

    [ScriptFunctionDefinition("setSkyGradient")]
    public bool SetSkyGradient(
        int topRed = 0, int topGreen = 0, int topBlue = 0,
        int bottomRed = 0, int bottomGreen = 0, int bottomBlue = 0)
    {
        gameWorld.SetSkyGradient(
            Color.FromArgb(topRed, topGreen, topBlue),
            Color.FromArgb(bottomRed, bottomGreen, bottomBlue)
        );
        return true;
    }

    [ScriptFunctionDefinition("resetSkyGradient")]
    public bool ResetSkyGradient()
    {
        gameWorld.SetSkyGradient(Color.FromArgb(0, 0, 0), Color.FromArgb(0, 0, 255));
        return true;
    }

    [ScriptFunctionDefinition("getSunColor")]
    public (int, int, int, int, int, int) GetSunColor()
    {
        var colors = gameWorld.GetSunColor();
        if (colors == null)
            return (255, 255, 255, 255, 200, 150);
        return (
            colors.Value.Item1.R, colors.Value.Item1.G, colors.Value.Item1.B,
            colors.Value.Item2.R, colors.Value.Item2.G, colors.Value.Item2.B
        );
    }

    [ScriptFunctionDefinition("setSunColor")]
    public bool SetSunColor(int coreR, int coreG, int coreB, int coronaR, int coronaG, int coronaB)
    {
        gameWorld.SetSunColor(
            Color.FromArgb(coreR, coreG, coreB),
            Color.FromArgb(coronaR, coronaG, coronaB)
        );
        return true;
    }

    [ScriptFunctionDefinition("resetSunColor")]
    public bool ResetSunColor()
    {
        gameWorld.SetSunColor(Color.FromArgb(255, 255, 255), Color.FromArgb(255, 200, 150));
        return true;
    }

    [ScriptFunctionDefinition("getSunSize")]
    public int GetSunSize()
        => gameWorld.SunSize;

    [ScriptFunctionDefinition("setSunSize")]
    public bool SetSunSize(int size)
    {
        gameWorld.SunSize = size;
        return true;
    }

    [ScriptFunctionDefinition("resetSunSize")]
    public bool ResetSunSize()
    {
        gameWorld.SunSize = 1;
        return true;
    }

    [ScriptFunctionDefinition("getRainLevel")]
    public float GetRainLevel()
        => gameWorld.RainLevel;

    [ScriptFunctionDefinition("setRainLevel")]
    public bool SetRainLevel(float level)
    {
        gameWorld.RainLevel = level;
        return true;
    }

    [ScriptFunctionDefinition("resetRainLevel")]
    public bool ResetRainLevel()
    {
        gameWorld.RainLevel = 0;
        return true;
    }

    [ScriptFunctionDefinition("getWindVelocity")]
    public (float, float, float) GetWindVelocity()
    {
        var v = gameWorld.WindVelocity;
        return (v.X, v.Y, v.Z);
    }

    [ScriptFunctionDefinition("setWindVelocity")]
    public bool SetWindVelocity(float x, float y, float z)
    {
        gameWorld.WindVelocity = new Vector3(x, y, z);
        return true;
    }

    [ScriptFunctionDefinition("resetWindVelocity")]
    public bool ResetWindVelocity()
    {
        gameWorld.WindVelocity = Vector3.Zero;
        return true;
    }

    [ScriptFunctionDefinition("getFarClipDistance")]
    public float GetFarClipDistance()
        => gameWorld.FarClipDistance ?? 300.0f;

    [ScriptFunctionDefinition("setFarClipDistance")]
    public bool SetFarClipDistance(float distance)
    {
        gameWorld.FarClipDistance = distance;
        return true;
    }

    [ScriptFunctionDefinition("resetFarClipDistance")]
    public bool ResetFarClipDistance()
    {
        gameWorld.FarClipDistance = null;
        return true;
    }

    [ScriptFunctionDefinition("getFogDistance")]
    public float GetFogDistance()
        => gameWorld.FogDistance ?? 0.0f;

    [ScriptFunctionDefinition("setFogDistance")]
    public bool SetFogDistance(float distance)
    {
        gameWorld.FogDistance = distance;
        return true;
    }

    [ScriptFunctionDefinition("resetFogDistance")]
    public bool ResetFogDistance()
    {
        gameWorld.FogDistance = null;
        return true;
    }

    [ScriptFunctionDefinition("getWeather")]
    public (int, int) GetWeather()
        => (gameWorld.Weather, gameWorld.PreviousWeather);

    [ScriptFunctionDefinition("setWeather")]
    public bool SetWeather(int weatherId)
    {
        gameWorld.SetWeather((byte)weatherId);
        return true;
    }

    [ScriptFunctionDefinition("setWeatherBlended")]
    public bool SetWeatherBlended(int weatherId)
    {
        gameWorld.SetWeatherBlended((byte)weatherId);
        return true;
    }
}
