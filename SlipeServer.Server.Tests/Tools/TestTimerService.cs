using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;

namespace SlipeServer.Server.Tests.Tools;

public class TestTimerService() : ITimerService
{
    private readonly List<Action> actions = [];

    public void CreateTimer(Action action, TimeSpan timespan) => actions.Add(action);

    public void TriggerTimers()
    {
        foreach (var action in actions)
            action();
    }
}
