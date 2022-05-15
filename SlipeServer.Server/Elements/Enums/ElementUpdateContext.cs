using System;

namespace SlipeServer.Server.Elements.Enums;

[Flags]
public enum ElementUpdateContext
{
    Relay = 0x00,
    NoRelay = 0x01,
    ServerOnly = 0x02 | NoRelay,
    PostEvent = 0x04 | Relay,

    Default = Relay,
    Sync = NoRelay,
}
