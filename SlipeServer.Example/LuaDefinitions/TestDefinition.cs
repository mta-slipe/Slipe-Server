using Microsoft.Extensions.Logging;
using SlipeServer.Scripting;

namespace SlipeServer.Example.LuaDefinitions;

public class TestDefinition
{
    private readonly ILogger logger;

    public TestDefinition(ILogger logger)
    {
        this.logger = logger;
    }

    [ScriptFunctionDefinition("callbackEqual")]
    public bool CallbackEqual(ScriptCallbackDelegateWrapper a, ScriptCallbackDelegateWrapper b)
    {
        this.logger.LogInformation($"{a} == {b} : {a.Equals(b)}");
        return a.Equals(b);
    }
}
