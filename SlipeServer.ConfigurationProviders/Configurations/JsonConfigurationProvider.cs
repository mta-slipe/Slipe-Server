using SlipeServer.Server;
using Newtonsoft.Json;
using System.IO;

namespace SlipeServer.ConfigurationProviders.Configurations;

public class JsonConfigurationProvider : IConfigurationProvider
{
    public Configuration configuration { private set; get; }
    public Configuration GetConfiguration() => configuration;
    public JsonConfigurationProvider(string fileName)
    {
        this.configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(fileName));
    }
}
