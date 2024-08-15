using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Packets.Structs;


namespace SlipeServer.Server.TestTools;

public struct SendLuaEvent
{
    public ulong Address { get; set; }
    public string Name { get; set; }
    public ElementId Source { get; set; }
    public LuaValue[] Arguments { get; set; }
}
