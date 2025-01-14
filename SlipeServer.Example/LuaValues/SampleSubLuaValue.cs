using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Events;
using SlipeServer.SourceGenerators;

namespace SlipeServer.Example.LuaValues;

[LuaValue]
public partial class SampleSubLuaValue : ILuaValue
{
    public string Header { get; set; } = null!;
    public string[] Messages { get; set; } = null!;

    public partial void Parse(LuaValue luaValue);
}
