using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlipeServer.Server.Elements.Events
{
    public class PlayerKickEventArgs : EventArgs
    {
        public string Reason { get; set; }
        public PlayerDisconnectType Type { get; set; }

        public PlayerKickEventArgs(string reason, PlayerDisconnectType type)
        {
            Reason = reason;
            Type = type;
        }
    }
}
