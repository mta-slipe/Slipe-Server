using SlipeServer.Server;
using Nett;

namespace SlipeServer.ConfigurationProviders.Configurations;

public class TomlConfigurationProvider : IConfigurationProvider
{
    public Configuration Configuration { private set; get; }
    public Configuration GetConfiguration() => this.Configuration;
    public TomlConfigurationProvider(string fileName)
    {
        this.Configuration = Toml.ReadFile<Configuration>(fileName);
    }
}
