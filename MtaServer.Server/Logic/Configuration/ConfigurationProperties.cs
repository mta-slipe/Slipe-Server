using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Logic.Configuration
{
    public class ConfigurationProperties
    {
        public string serverName { get; protected set; }
        public string host { get; protected set; }
        public ushort port { get; protected set; }
        public ushort maxPlayers { get; protected set; }
        public string password { get; protected set; }
    }
}
