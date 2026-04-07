using SlipeServer.Packets.Definitions.Lua;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting.Definitions;

public readonly record struct ScriptTimerDetails(double Remaining, int ExecutesRemaining, int Interval);

public class TimerScriptDefinitions(ScriptTimerService timerService)
{
    [ScriptFunctionDefinition("setTimer")]
    public ScriptTimer SetTimer(ScriptCallbackDelegateWrapper callback, int intervalMs, int timesToExecute, params LuaValue[] arguments)
    {
        return timerService.CreateTimer(callback, intervalMs, timesToExecute, arguments);
    }

    [ScriptFunctionDefinition("killTimer")]
    public bool KillTimer(ScriptTimer timer)
    {
        return timerService.KillTimer(timer);
    }

    [ScriptFunctionDefinition("resetTimer")]
    public bool ResetTimer(ScriptTimer timer)
    {
        return timerService.ResetTimer(timer);
    }

    [ScriptFunctionDefinition("isTimer")]
    public bool IsTimer(ScriptTimer? timer = null)
    {
        return timerService.IsValidTimer(timer);
    }

    [ScriptFunctionDefinition("isTimerPaused")]
    public bool IsTimerPaused(ScriptTimer timer)
    {
        return timer.IsPaused;
    }

    [ScriptFunctionDefinition("setTimerPaused")]
    public bool SetTimerPaused(ScriptTimer timer, bool paused)
    {
        if (!timer.IsAlive)
            return false;

        timer.SetPaused(paused);
        return true;
    }

    [ScriptFunctionDefinition("getTimers")]
    public IEnumerable<ScriptTimer> GetTimers(int? maxRemainingMs = null)
    {
        return timerService.GetTimers(maxRemainingMs);
    }

    [ScriptFunctionDefinition("getTimerDetails")]
    public ScriptTimerDetails GetTimerDetails(ScriptTimer timer)
    {
        return new ScriptTimerDetails(timer.GetRemainingMs(), timer.ExecutionsRemaining == int.MaxValue ? 0 : timer.ExecutionsRemaining, timer.IntervalMs);
    }
}
