using System;

namespace MtaServer.Server.AllSeeingEye
{
    [Flags]
    public enum PlayerFlags
    {
        Nick = 0x01,
        Team = 0x02,
        Skin = 0x04,
        Score = 0x08,
        Ping = 0x16,
        Time = 0x32,
    }
}
