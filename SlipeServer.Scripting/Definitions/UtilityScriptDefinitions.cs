using Microsoft.Extensions.Logging;
using SlipeServer.Server;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Services;
using System;
using System.Collections;
using System.Drawing;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

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
            try
            {
                return ColorTranslator.FromHtml(color);
            }
            catch (Exception)
            {
                return null;
            }
        }

        [ScriptFunctionDefinition("md5")]
        public string CreateMD5(string input)
        {
            MD5 md5 = MD5.Create();
            
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        [ScriptFunctionDefinition("sha256")]
        public string Sha256(string input)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            var sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("X2"));
            }

            return sBuilder.ToString();
        }
    }
}
