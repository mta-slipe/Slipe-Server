using System;
using System.Timers;

namespace SlipeServer.Server.Services;

/// <summary>
/// Interface that allows you to create arbitrary timers, this exists so time-based logic can be tested using mocking.
/// </summary>
public interface ITimerService
{
    void CreateTimer(Action action, TimeSpan timespan);
}

internal class TimerService : ITimerService
{
    public void CreateTimer(Action action, TimeSpan timespan)
    {
        var timer = new Timer(timespan.TotalMilliseconds)
        {
            AutoReset = true,
        };
        timer.Start();
        timer.Elapsed += (sender, args) => action();
    }
}
