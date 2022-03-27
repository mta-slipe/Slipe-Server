using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Events;

public class PlayerKickEventArgs : EventArgs
{
    public string Reason { get; set; }
    public PlayerDisconnectType Type { get; set; }

    public PlayerKickEventArgs(string reason, PlayerDisconnectType type)
    {
        this.Reason = reason;
        this.Type = type;
    }
}
