using SlipeServer.Server.Resources;
using System.Collections.Generic;

namespace SlipeServer.Scripting;

public interface IScriptTimerService
{
    ScriptTimer CreateTimer(ScriptCallbackDelegateWrapper callback, int intervalMs, int timesToExecute, object?[] arguments);
    bool KillTimer(ScriptTimer timer);
    bool ResetTimer(ScriptTimer timer);
    IEnumerable<ScriptTimer> GetTimers(int? maxRemainingMs = null);
    bool IsValidTimer(ScriptTimer? timer);
    void KillTimersWithContext(Resource? owningResource);
}
