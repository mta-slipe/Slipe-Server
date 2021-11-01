using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Events;
using SlipeServer.SourceGenerators;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace SlipeServer.Console.LuaValues
{
    [LuaValue]
    public partial class SampleLuaValue
    {
        public float Float { get; set; }
        public float? OptionalFloat { get; set; }
        public double Double { get; set; }
        public double? OptionalDouble { get; set; }
        public Vector3 Position { get; set; }
        public Vector3? OptionalPosition { get; set; }
        public int Integer { get; set; }
        public int? OptionalInteger { get; set; }

        [Required]
        public string Text { get; set; } = null!;

        public bool Boolean { get; set; }
        public bool? OptionalBoolean { get; set; }

        public SampleSubLuaValue SubValue { get; set; }

        public partial void Parse(LuaValue luaEvent);
    }
}
