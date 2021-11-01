using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Events;
using SlipeServer.SourceGenerators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;

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
