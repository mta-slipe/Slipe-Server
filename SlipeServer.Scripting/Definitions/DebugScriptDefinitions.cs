using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System;
using System.Numerics;

namespace SlipeServer.Scripting.Definitions
{
    public class DebugScriptDefinitions
    {
        private readonly DebugLog debugLog;
        private readonly ILogger logger;

        public DebugScriptDefinitions(DebugLog debugLog, ILogger logger)
        {
            this.debugLog = debugLog;
            this.logger = logger;
        }

        [ScriptFunctionDefinition("outputDebugString")]
        public void OutputDebugString(string message)
        {
            this.debugLog.Output(message);
            this.logger.LogDebug(message);
        }


    }
}
