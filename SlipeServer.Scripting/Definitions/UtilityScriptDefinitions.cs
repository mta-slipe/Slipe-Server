using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System;
using System.Collections;
using System.Drawing;
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
        
        [ScriptFunctionDefinition("base64Encode")]
        public string Base64Encode(string data)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
        }

        [ScriptFunctionDefinition("base64Decode")]
        public string Base64Decode(string data)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data));
        }

        [ScriptFunctionDefinition("tocolor")]
        public int ToColor(byte r, byte g, byte b, byte a = 255)
        {
            return b + g * 256 + r * 256 * 256 + a * 256 * 256 * 256;
        }

        [ScriptFunctionDefinition("getColorFromString")]
        public Color? GetColorFromString(string color)
        {
            return ColorTranslator.FromHtml(color);
        }
    }
}
