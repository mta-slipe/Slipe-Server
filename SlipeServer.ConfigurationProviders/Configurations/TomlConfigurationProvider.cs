using SlipeServer.Server;
using Nett;

namespace SlipeServer.ConfigurationProviders
{
    public class TomlConfigurationProvider : IConfigurationProvider
    {
        public Configuration configuration { private set; get; }
        public Configuration GetConfiguration() => configuration;
        public TomlConfigurationProvider(string fileName)
        {
            this.configuration = Toml.ReadFile<Configuration>(fileName);
        }
    }
}
