using Microsoft.Extensions.Logging;

namespace SlipeServer.Scripting.Definitions;

public class AssertionScriptDefinition
{
    private readonly ILogger<AssertionScriptDefinition> logger;
    private readonly IScriptingRuntimeService scriptingService;

    public AssertionScriptDefinition(ILogger<AssertionScriptDefinition> logger, IScriptingRuntimeService scriptingService)
    {
        this.logger = logger;
        this.scriptingService = scriptingService;
    }

    [ScriptFunctionDefinition("assertEqual")]
    public void AssertEqual(object a, object b, string? because = null)
    {
        var same = this.scriptingService.Compare(a, b);
        if (!same)
        {
            if(because != null)
            {
                this.logger.LogError("Assertion failed, {a} != {b} because {because}", this.scriptingService.ToDebugString(a), this.scriptingService.ToDebugString(b), because);
            } else
            {
                this.logger.LogError("Assertion failed, {a} != {b}", this.scriptingService.ToDebugString(a), this.scriptingService.ToDebugString(b));
            }
        }
    }

}
