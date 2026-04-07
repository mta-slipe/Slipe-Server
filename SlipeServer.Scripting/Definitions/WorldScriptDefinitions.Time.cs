namespace SlipeServer.Scripting.Definitions;

public partial class WorldScriptDefinitions
{
    [ScriptFunctionDefinition("getTime")]
    public (int, int) GetTime()
    {
        var time = gameWorld.GetTime();
        return (time.Item1, time.Item2);
    }

    [ScriptFunctionDefinition("setTime")]
    public bool SetTime(int hour, int minute)
    {
        gameWorld.SetTime((byte)hour, (byte)minute);
        return true;
    }

    [ScriptFunctionDefinition("getMinuteDuration")]
    public uint GetMinuteDuration()
        => gameWorld.MinuteDuration;

    [ScriptFunctionDefinition("setMinuteDuration")]
    public bool SetMinuteDuration(uint duration)
    {
        gameWorld.MinuteDuration = duration;
        return true;
    }

    [ScriptFunctionDefinition("getGameSpeed")]
    public float GetGameSpeed()
        => gameWorld.GameSpeed;

    [ScriptFunctionDefinition("setGameSpeed")]
    public bool SetGameSpeed(float speed)
    {
        gameWorld.GameSpeed = speed;
        return true;
    }
}
