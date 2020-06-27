using MtaServer.Server;
using Newtonsoft.Json;
using System.IO;

namespace MtaServer.ConfigurationProviders
{
    public class JsonConfigurationProvider : IConfigurationProvider
    {
        public Configuration configuration { get; private set; }
        public Configuration GetConfiguration() => configuration;
        public JsonConfigurationProvider(string fileName)
        {
            this.configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(fileName));
        }
    }
}
