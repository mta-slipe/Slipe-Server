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

        public ushort MaxPlayerCount { get; set; } = 64;

        [MaxLength(128)]
        public string Password { get; set; } = "";


        public string HttpHost { get; set; } = "127.0.0.1";

        public ushort HttpPort { get; set; } = 50680;

        public string? HttpUrl { get; set; }

        public int HttpConnectionsPerClient { get; set; } = 1;

        public string ResourceDirectory { get; set; } = "./resources";

    }
}
