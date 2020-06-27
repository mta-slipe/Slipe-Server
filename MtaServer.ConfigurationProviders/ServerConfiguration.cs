using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MtaServer.ConfigurationProviders
{
    public class ServerConfiguration
    {
        private Configuration configuration;
        public ServerConfiguration()
        {
            configuration = new Configuration();
        }

        public ServerConfiguration(Configuration configuration)
        {
            this.configuration = configuration;
        }

        public bool Verify(out string property)
        {
            if (configuration.serverName.Length > 128)
            {
                property = "serverName";
                return false;
            }
            if (configuration.password.Length > 128)
            {
                property = "password";
                return false;
            }

            if (IPAddress.TryParse(configuration.host, out IPAddress address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        break;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                    default:
                        property = "host";
                        return false;
                }
            }
            property = "";
            return true;
        }

        public string GetHost() => configuration.host;
        public ushort GetPort() => configuration.port;

        public bool SetServerName(string newServerName)
        {
            if (string.IsNullOrEmpty(newServerName))
                return false;

            if (newServerName.Length > 128)
                return false;

            configuration.serverName = newServerName;
            OnConfigurationChanged?.Invoke("serverName", configuration.serverName);
            return true;
        }
        
        public bool SetPassword(string newPassword)
        {
            if (newPassword.Length > 128)
                return false;

            configuration.password = newPassword;
            OnConfigurationChanged?.Invoke("password", configuration.password);
            return true;
        }
        
        private bool SetPort(ushort newPort)
        {
            configuration.port = newPort;
            return true;
        }
        
        public bool SetMaxPlayers(ushort newMaxPlayers)
        {
            if (newMaxPlayers == 0)
                return false;

            configuration.maxPlayers = newMaxPlayers;
            OnConfigurationChanged?.Invoke("maxPlayers", configuration.maxPlayers);
            return true;
        }
        
        private bool SetHost(string newHost)
        {
            if (string.IsNullOrEmpty(newHost))
                return false;

            if (IPAddress.TryParse(newHost, out IPAddress address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        break;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                    default:
                        return false;
                }
            }

            configuration.host = newHost;
            return true;
        }

        public delegate void ConfigurationChangedHandler(string configName, object newValue);
        public event ConfigurationChangedHandler OnConfigurationChanged;
    }
}
