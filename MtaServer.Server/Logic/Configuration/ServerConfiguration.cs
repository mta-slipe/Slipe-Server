using MtaServer.Server.Exceptions;
using MtaServer.Server.Logic.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MtaServer.Server.Logic
{
    public class ServerConfiguration : ConfigurationProperties
    {
        public ServerConfiguration()
        {
            DefaultConfiguration configurationProvider = new DefaultConfiguration();
            LoadConfiguration(configurationProvider);
        }

        public ServerConfiguration(IConfigurationProvider configurationProvider)
        {
            LoadConfiguration(configurationProvider);
        }

        private void LoadConfiguration(IConfigurationProvider configurationProvider)
        {
            if (!SetServerName(configurationProvider.GetServerName()))
                throw new ConfigurationException("serverName");
            if (!SetHost(configurationProvider.GetHost()))
                throw new ConfigurationException("host");
            if (!SetPort(configurationProvider.GetPort()))
                throw new ConfigurationException("port");
            if (!SetMaxPlayers(configurationProvider.GetMaxPlayers()))
                throw new ConfigurationException("maxPlayers");
            if (!SetPassword(configurationProvider.GetPassword()))
                throw new ConfigurationException("password");
        }
        public bool SetServerName(string newServerName)
        {
            if (string.IsNullOrEmpty(newServerName))
                return false;

            if (newServerName.Length > 128)
                return false;

            serverName = newServerName;
            OnConfigurationChanged?.Invoke("serverName", serverName);
            return true;
        }
        
        public bool SetPassword(string newPassword)
        {
            if (newPassword.Length > 128)
                return false;

            password = newPassword;
            OnConfigurationChanged?.Invoke("password", newPassword);
            return true;
        }
        
        private bool SetPort(ushort newPort)
        {
            port = newPort;
            return true;
        }
        
        public bool SetMaxPlayers(ushort newMaxPlayers)
        {
            if (newMaxPlayers == 0)
                return false;
            maxPlayers = newMaxPlayers;
            OnConfigurationChanged?.Invoke("maxPlayers", maxPlayers);
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

            host = newHost;
            return true;
        }

        public delegate void ConfigurationChangedHandler(string configName, object newValue);
        public event ConfigurationChangedHandler OnConfigurationChanged;
    }
}
