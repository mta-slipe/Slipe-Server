using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.SourceGenerators;

namespace SlipeServer.Console.LuaValues
{
    [LuaValue]
    public partial class SampleSubLuaValue
    {
        public string Header { get; set; } = null!;
        public string[] Messages { get; set; } = null!;

        public partial void Parse(LuaValue luaValue);
    }
}
