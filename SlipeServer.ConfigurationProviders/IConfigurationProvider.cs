using SlipeServer.Server;

namespace SlipeServer.ConfigurationProviders;

public interface IConfigurationProvider
{
    public Configuration GetConfiguration();
}
