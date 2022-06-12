using SlipeServer.Server;
using Newtonsoft.Json;
using System.IO;

namespace SlipeServer.ConfigurationProviders.Configurations;

public class JsonConfigurationProvider : IConfigurationProvider
{
    public Configuration Configuration { private set; get; }
    public Configuration GetConfiguration() => this.Configuration;
    public JsonConfigurationProvider(string fileName)
    {
        this.Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(fileName));
    }
}
