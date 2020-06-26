using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Logic.Configuration
{
    public interface IConfigurationProvider
    {
        public string GetServerName();
        public string GetHost();
        public ushort GetPort();
        public ushort GetMaxPlayers();
        public string GetPassword();
    }
}
