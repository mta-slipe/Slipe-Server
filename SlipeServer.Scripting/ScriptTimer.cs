using System;
using System.Linq;
using System.Timers;

namespace SlipeServer.Scripting;

public class ScriptTimer
{
    private static int nextId = 1;

    public int Id { get; } = nextId++;
    public ScriptCallbackDelegateWrapper Callback { get; }
    public object?[] Arguments { get; }
    public int IntervalMs { get; }
    public int TimesToExecute { get; }
    public int ExecutionsRemaining { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsAlive { get; private set; }
    public ScriptExecutionContext? ExecutionContext { get; init; }

    private readonly Timer timer;
    private double pausedRemainingMs;
    private DateTime lastFireTime;

    public ScriptTimer(ScriptCallbackDelegateWrapper callback, int intervalMs, int timesToExecute, object?[] arguments)
    {
        this.Callback = callback;
        this.IntervalMs = intervalMs;
        this.TimesToExecute = timesToExecute;
        this.ExecutionsRemaining = timesToExecute == 0 ? int.MaxValue : timesToExecute;
        this.Arguments = arguments;
        this.IsAlive = true;

        this.timer = new Timer(intervalMs)
        {
            AutoReset = timesToExecute != 1
        };
        this.timer.Elapsed += OnElapsed;
        this.timer.Start();
        this.lastFireTime = DateTime.UtcNow;
    }

    private void OnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!this.IsAlive || this.IsPaused)
            return;

        this.lastFireTime = DateTime.UtcNow;

        try
        {
            this.Callback.CallbackDelegate(this.Arguments.Cast<object?>().ToArray());
        }
        catch { }

        if (this.TimesToExecute != 0)
        {
            this.ExecutionsRemaining--;
            if (this.ExecutionsRemaining <= 0)
                Kill();
        }
    }

    public void Kill()
    {
        this.IsAlive = false;
        this.timer.Stop();
        this.timer.Dispose();
    }

    public void Reset()
    {
        this.timer.Stop();
        this.ExecutionsRemaining = this.TimesToExecute == 0 ? int.MaxValue : this.TimesToExecute;
        this.lastFireTime = DateTime.UtcNow;
        this.timer.Interval = this.IntervalMs;
        this.timer.Start();
    }

    public void SetPaused(bool paused)
    {
        if (paused == this.IsPaused)
            return;

        if (paused)
        {
            this.pausedRemainingMs = Math.Max(0, this.IntervalMs - (DateTime.UtcNow - this.lastFireTime).TotalMilliseconds);
            this.timer.Stop();
        }
        else
        {
            this.timer.Interval = this.pausedRemainingMs > 0 ? this.pausedRemainingMs : this.IntervalMs;
            this.timer.Start();
            this.lastFireTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(this.IntervalMs - this.timer.Interval);
        }

        this.IsPaused = paused;
    }

    public double GetRemainingMs()
    {
        if (!this.IsAlive)
            return 0;
        var elapsed = (DateTime.UtcNow - this.lastFireTime).TotalMilliseconds;
        return Math.Max(0, this.IntervalMs - elapsed);
    }
}
