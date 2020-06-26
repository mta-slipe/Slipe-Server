using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Exceptions
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string config) : base($"Invalid configuration value for {config}")
        {

        }
    }
}
