using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Logic.Configuration
{

    public class DefaultConfiguration : ConfigurationProperties, IConfigurationProvider
    {
        public DefaultConfiguration()
        {
            serverName = "Default Slipe server";
            host = "0.0.0.0";
            port = 50666;
            maxPlayers = 32;
            password = "";
        }

        public string GetServerName() => serverName;
        public string GetHost() => host;
        public ushort GetPort() => port;
        public ushort GetMaxPlayers() => maxPlayers;
        public string GetPassword() => password;
    }
}
