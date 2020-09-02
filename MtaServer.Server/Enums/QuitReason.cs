using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Enums
{
    public enum QuitReason
    {
        Quit,
        Kick,
        Ban,
        ConnectionDesync,
        Timeout,
    }
}
