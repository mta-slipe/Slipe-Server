using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using SlipeServer.Server.Services;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipeServer.Scripting.Definitions;

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

    [ScriptFunctionDefinition("iprint")]
    public void Print(IEnumerable<object> toPrint)
    {
        toPrint = (List<object>)toPrint;
        StringBuilder sb = new StringBuilder();
        int i = 0;
        int len = toPrint.Count();
        foreach (var printable in toPrint)
        {
            string? stringRepresentation = printable.ToString();
            sb.Append(stringRepresentation ?? "NoStringRepresentation");
            if (i != len-1)
            {
                sb.Append(", ");
            }
            i++;
        }
        string message = sb.ToString();
        this.debugLog.Output(message);
        this.logger.LogInformation(message);
    }
}
