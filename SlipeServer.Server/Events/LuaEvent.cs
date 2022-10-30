using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Elements;

namespace SlipeServer.Server.Events;

public struct LuaEvent
{
    public Player Player { get; set; }
    public Element Source { get; set; }
    public string Name { get; set; }
    public LuaValue[] Parameters { get; set; }
}
