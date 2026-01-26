using SlipeServer.Server;
using Nett;

namespace SlipeServer.ConfigurationProviders.Configurations;

public class TomlConfigurationProvider(string fileName) : IConfigurationProvider
{
    public Configuration Configuration { private set; get; } = Toml.ReadFile<Configuration>(fileName);
    public Configuration GetConfiguration() => this.Configuration;
}
