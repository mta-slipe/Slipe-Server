using MtaServer.Packets.Definitions.Lua;
using MtaServer.Server.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtaServer.Server.Events
{
    public struct LuaEvent
    {
        public Player Player { get; set; }
        public Element Source { get; set; }
        public string Name { get; set; }
        public LuaValue[] Parameters { get; set; }
    }
}
