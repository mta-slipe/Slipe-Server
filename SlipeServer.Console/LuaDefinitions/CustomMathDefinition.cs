using Microsoft.Extensions.Logging;
using SlipeServer.Scripting;
using SlipeServer.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Console.LuaDefinitions
{
    public class CustomMathDefinition
    {
        private readonly DebugLog debugLog;
        private readonly ILogger logger;

        public CustomMathDefinition(DebugLog debugLog, ILogger logger)
        {
            this.debugLog = debugLog;
            this.logger = logger;
        }

        [ScriptFunctionDefinition("add")]
        public int Add(int a, int b)
        {
            return a + b;
        }

        [ScriptFunctionDefinition("substract")]
        public int Substract(int a, int b)
        {
            return a - b;
        }
    }
}
