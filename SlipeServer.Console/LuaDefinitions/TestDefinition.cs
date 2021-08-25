using Microsoft.Extensions.Logging;
using SlipeServer.Console.LuaDefinitions.Userdatas;
using SlipeServer.Lua;
using SlipeServer.Scripting;
using SlipeServer.Server.Services;

namespace SlipeServer.Console.LuaDefinitions
{
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

        [ScriptFunctionDefinition("createFoo")]
        public FooUserdata CreateFoo(int a, int b)
        {
            return new FooUserdata
            {
                A = a,
                B = b,
            };
        }

        [ScriptFunctionDefinition("sumFoo")]
        public int SumFoo(FooUserdata foo)
        {
            return foo.A + foo.B;
        }
    }
}
