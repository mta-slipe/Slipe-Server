using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Resources;
using System.Collections.Generic;
using System.Linq;

namespace SlipeServer.Scripting;

public class ScriptTimerService
{
    private readonly List<ScriptTimer> timers = [];

    public ScriptTimer CreateTimer(ScriptCallbackDelegateWrapper callback, int intervalMs, int timesToExecute, LuaValue[] arguments)
    {
        var timer = new ScriptTimer(callback, intervalMs, timesToExecute, arguments)
        {
            ExecutionContext = ScriptExecutionContext.Current
        };

        this.timers.Add(timer);
        return timer;
    }

    public bool KillTimer(ScriptTimer timer)
    {
        if (!timer.IsAlive)
            return false;

        timer.Kill();
        this.timers.Remove(timer);
        return true;
    }

    public bool ResetTimer(ScriptTimer timer)
    {
        if (!timer.IsAlive)
            return false;

        timer.Reset();
        return true;
    }

    public IEnumerable<ScriptTimer> GetTimers(int? maxRemainingMs = null)
    {
        var alive = this.timers.Where(t => t.IsAlive).ToList();

        if (maxRemainingMs.HasValue)
            return alive.Where(t => t.GetRemainingMs() <= maxRemainingMs.Value);

        return alive;
    }

    public bool IsValidTimer(ScriptTimer? timer) => timer != null && timer.IsAlive;

    public void KillTimersWithContext(Resource? owningResource)
    {
        var timers = this.timers
            .Where(t => t.ExecutionContext?.Owner == owningResource)
            .ToArray();

        foreach (var timer in timers)
        {
            timer.Kill();
            this.timers.Remove(timer);
        }
    }
}
