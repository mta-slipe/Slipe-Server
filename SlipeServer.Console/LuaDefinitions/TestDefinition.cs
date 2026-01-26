using Microsoft.Extensions.Logging;
using SlipeServer.Scripting;

namespace SlipeServer.Console.LuaDefinitions;

public class TestDefinition(ILogger logger)
{
    [ScriptFunctionDefinition("callbackEqual")]
    public bool CallbackEqual(ScriptCallbackDelegateWrapper a, ScriptCallbackDelegateWrapper b)
    {
        logger.LogInformation($"{a} == {b} : {a.Equals(b)}");
        return a.Equals(b);
    }
}
