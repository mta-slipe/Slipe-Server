using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Packets.Enums
{
    public enum RpcFunctions
    {
        PLAYER_INGAME_NOTICE,
        INITIAL_DATA_STREAM,
        PLAYER_TARGET,
        PLAYER_WEAPON,
        KEY_BIND,
        CURSOR_EVENT,
        REQUEST_STEALTH_KILL,
    }
}