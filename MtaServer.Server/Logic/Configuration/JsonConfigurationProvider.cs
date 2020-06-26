using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MtaServer.Server.Logic.Configuration
{
    public class JsonConfigurationProvider : ConfigurationProperties, IConfigurationProvider
    {
        public JsonConfigurationProvider(string fileName)
        {
            JObject jsonObjectConfig = JObject.Parse(File.ReadAllText(fileName));
            Dictionary<string, string> dictObj = jsonObjectConfig.ToObject<Dictionary<string, string>>();

            string value;
            if (dictObj.TryGetValue("serverName", out value))
                serverName = value;
            if (dictObj.TryGetValue("host", out value))
                host = value;
            if (dictObj.TryGetValue("port", out value))
                if(ushort.TryParse(value, out ushort result))
                    port = result;

            if (dictObj.TryGetValue("maxPlayers", out value))
                if (ushort.TryParse(value, out ushort result))
                    maxPlayers = result;

            if (dictObj.TryGetValue("password", out value))
                password = value;

        }

        public string GetServerName() => serverName;
        public string GetHost() => host;
        public ushort GetPort() => port;
        public ushort GetMaxPlayers() => maxPlayers;
        public string GetPassword() => password;
    }
}
