using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.ConfigurationProviders
{
    public interface IConfigurationProvider
    {
        public Configuration GetConfiguration();
    }
}
