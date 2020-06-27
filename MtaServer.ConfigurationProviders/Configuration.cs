using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MtaServer.ConfigurationProviders
{
    public class Configuration
    {
        [MaxLength(128)]
        public string serverName { get; set; } = "Default Slipe server";
        public string host { get; set; } = "0.0.0.0";
        public ushort port { get; set; } = 50666;
        public ushort maxPlayers { get; set; } = 32;
        public string password { get; set; } = "";
    }
}
