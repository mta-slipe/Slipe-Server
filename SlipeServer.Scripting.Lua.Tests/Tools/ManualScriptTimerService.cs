using SlipeServer.Server.Resources;

namespace SlipeServer.Scripting.Lua.Tests.Tools;

/// <summary>
/// A testable implementation of <see cref="IScriptTimerService"/> that does not use real-time timers.
/// Call <see cref="FireAllTimers"/> or <see cref="FireTimersWithMaxInterval"/> to manually trigger callbacks.
/// </summary>
public class ManualScriptTimerService : IScriptTimerService
{
    private readonly List<ScriptTimer> timers = [];

    public ScriptTimer CreateTimer(ScriptCallbackDelegateWrapper callback, int intervalMs, int timesToExecute, object?[] arguments)
    {
        var timer = ScriptTimer.CreateManual(callback, intervalMs, timesToExecute, arguments);
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
        return this.timers.Where(t => t.IsAlive).ToList();
    }

    public bool IsValidTimer(ScriptTimer? timer) => timer != null && timer.IsAlive;

    public void KillTimersWithContext(Resource? owningResource)
    {
        var toRemove = this.timers
            .Where(t => t.ExecutionContext?.Owner == owningResource)
            .ToArray();

        foreach (var timer in toRemove)
        {
            timer.Kill();
            this.timers.Remove(timer);
        }
    }

    /// <summary>
    /// Fires all live timers once, regardless of interval.
    /// </summary>
    public void FireAllTimers()
    {
        foreach (var timer in this.timers.Where(t => t.IsAlive).ToList())
            timer.FireManually();

        this.timers.RemoveAll(t => !t.IsAlive);
    }

    /// <summary>
    /// Fires all live timers whose interval is at or below <paramref name="maxIntervalMs"/>.
    /// </summary>
    public void FireTimersWithMaxInterval(int maxIntervalMs)
    {
        foreach (var timer in this.timers.Where(t => t.IsAlive && t.IntervalMs <= maxIntervalMs).ToList())
            timer.FireManually();

        this.timers.RemoveAll(t => !t.IsAlive);
    }
}
