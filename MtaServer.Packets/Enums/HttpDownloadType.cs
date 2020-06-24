using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Enums
{
    public enum HttpDownloadType
    {
        HTTP_DOWNLOAD_DISABLED = 0,
        HTTP_DOWNLOAD_ENABLED_PORT,
        HTTP_DOWNLOAD_ENABLED_URL
    }
}