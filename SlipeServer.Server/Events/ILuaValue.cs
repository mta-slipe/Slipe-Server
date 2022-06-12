using SlipeServer.Packets.Definitions.Lua;

namespace SlipeServer.Server.Events;

public interface ILuaValue
{
    void Parse(LuaValue luaValue);
}
