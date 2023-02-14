using SlipeServer.Packets.Definitions.Lua;

namespace SlipeServer.Packets.Definitions.Entities.Structs;

public struct CustomDataItem
{
    public string Name { get; set; }
    public LuaValue Data { get; set; }
}
