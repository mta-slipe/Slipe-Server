using MtaServer.Server;

namespace MtaServer.ConfigurationProviders
{
    public interface IConfigurationProvider
    {
        public Configuration GetConfiguration();
    }
}
