using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions
{
    public class UtilityScriptDefinitions
    {
        private readonly DebugLog debugLog;
        private readonly ILogger logger;
        private readonly DateTime start;

        public UtilityScriptDefinitions(DebugLog debugLog, ILogger logger)
        {
            this.debugLog = debugLog;
            this.logger = logger;
            this.start = DateTime.Now;
        }

        [ScriptFunctionDefinition("getTickCount")]
        public double GetTickCount()
        {
            return Math.Floor((DateTime.Now - start).TotalMilliseconds + 0.5);
        }


    }
}
