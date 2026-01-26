using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Mappers;
using System.Collections.Generic;

namespace SlipeServer.Server.Tests.Unit.Mappers;

internal struct LuaMappableVector3(float x, float y, float z) : ILuaMappable
{
    public float X { get; set; } = x;
    public float Y { get; set; } = y;
    public float Z { get; set; } = z;

    public LuaValue ToLuaValue() => new Dictionary<LuaValue, LuaValue>()
    {
        ["X"] = this.X * 2,
        ["Y"] = this.Y * 2,
        ["Z"] = this.Z * 2,
    };
}
