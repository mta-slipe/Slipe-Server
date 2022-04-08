using System;
using System.Collections.Generic;
using System.Text;

namespace SlipeServer.Server.Enums;

public enum QuitReason
{
    Quit,
    Kick,
    Ban,
    ConnectionDesync,
    Timeout,
}
