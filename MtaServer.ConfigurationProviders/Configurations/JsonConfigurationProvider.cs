using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MtaServer.ConfigurationProviders
{
    public class JsonConfigurationProvider : IConfigurationProvider
    {
        public Configuration configuration { private set; get; }
        public Configuration GetConfiguration() => configuration;
        public JsonConfigurationProvider(string fileName)
        {

            JObject jsonObjectConfig = JObject.Parse(File.ReadAllText(fileName));
            Dictionary<string, string> dictObj = jsonObjectConfig.ToObject<Dictionary<string, string>>();

            string value;
            if (dictObj.TryGetValue("serverName", out value))
                configuration.serverName = value;
            if (dictObj.TryGetValue("host", out value))
                configuration.host = value;
            if (dictObj.TryGetValue("port", out value))
                if(ushort.TryParse(value, out ushort result))
                    configuration.port = result;

            if (dictObj.TryGetValue("maxPlayers", out value))
                if (ushort.TryParse(value, out ushort result))
                    configuration.maxPlayers = result;

            if (dictObj.TryGetValue("password", out value))
                configuration.password = value;

        }
    }
}
