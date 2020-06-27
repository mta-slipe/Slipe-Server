using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MtaServer.Server
{
    public class Configuration
    {
        [MaxLength(128)]
        public string ServerName { get; set; } = "Default Slipe server";
        [MaxLength(15)]
        public string Host { get; set; } = "0.0.0.0";
        public ushort Port { get; set; } = 50666;
        public ushort MaxPlayers { get; set; } = 32;
        [MaxLength(128)]
        public string Password { get; set; } = "";
    }
}
