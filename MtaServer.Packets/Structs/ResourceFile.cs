using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Structs
{
    public struct ResourceFile
    {
        public string Name { get; set; }
        public byte FileType { get; set; }
        public ulong CheckSum { get; set; }
        public byte[] Md5 { get; set; }
        public double AproximateSize { get; set; }
        public bool? IsAutoDownload { get; set; }
    }
}
