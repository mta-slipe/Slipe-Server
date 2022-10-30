using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Mappers;
using System.Collections.Generic;

namespace SlipeServer.Server.Tests.Unit.Mappers;

internal struct LuaMappableVector3 : ILuaMappable
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public LuaMappableVector3(float x, float y, float z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public LuaValue ToLuaValue() => new Dictionary<LuaValue, LuaValue>()
    {
        ["X"] = this.X * 2,
        ["Y"] = this.Y * 2,
        ["Z"] = this.Z * 2,
    };
}
